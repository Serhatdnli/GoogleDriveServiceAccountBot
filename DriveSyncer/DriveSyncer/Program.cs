using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Google.Apis.Requests.BatchRequest;
using static System.Net.WebRequestMethods;

namespace DriveSyncer
{
	class Program
	{
		static string FilePath;
		static string DriveRootFolderId;

		static string setupPath = @"\Setup.txt";

		static DriveService service;

		static void Main(string[] args)
		{

			string[] setup = System.IO.File.ReadAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + setupPath).Split(',');
			FilePath = setup[0];
			DriveRootFolderId = setup[1];

			TransferToFile();
			Console.ReadKey();

		}


		public static async Task<string> DeleteItem(string itemId)
		{
			var response = await service.Files.Delete(itemId).ExecuteAsync();
			return response;
		}

		//public static List<string> GetFolderNames() => Directory.GetDirectories(FilePath).ToList();
		public static async Task<Google.Apis.Upload.IUploadProgress> UploadFile(string filePath, string mimeType, string parentId)
		{


			FileInfo fileInfo = new FileInfo(filePath);
			DateTime localCreatedTime = fileInfo.CreationTime;
			DateTime localModifiedTime = fileInfo.LastWriteTime;

			var excistItemMetadata = new Google.Apis.Drive.v3.Data.File()
			{
				MimeType = mimeType,
				Name = Path.GetFileName(filePath),
				Parents = new List<string>
				{
					parentId
				},
				CreatedTime = localCreatedTime,
				ModifiedTime = localModifiedTime


			};

			using (var stream = new FileStream(filePath, FileMode.Open))
			{
				var itemrequest = service.Files.Create(body: excistItemMetadata, stream, mimeType);
				var itemresponse = await itemrequest.UploadAsync();


				return itemresponse;


			}

		}

		public static async Task<Google.Apis.Drive.v3.Data.File> CreateFile(string filePath, string mimeType, string parentId)
		{
			FileInfo fileInfo = new FileInfo(filePath);
			DateTime localCreatedTime = fileInfo.CreationTime;
			DateTime localModifiedTime = fileInfo.LastWriteTime;

			var excistItemMetadata = new Google.Apis.Drive.v3.Data.File()
			{
				MimeType = mimeType,
				Name = Path.GetFileName(filePath),
				Parents = new List<string>
				{
					parentId
				},
				CreatedTime = localCreatedTime,
				ModifiedTime = localModifiedTime

			};


			var itemrequest = service.Files.Create(body: excistItemMetadata);
			var itemresponse = await itemrequest.ExecuteAsync();



			return itemresponse;

		}

		public static async Task<IList<Google.Apis.Drive.v3.Data.File>> GetFiles(string parentId)
		{
			var request = service.Files.List();
			request.Q = $"'{parentId}' in parents";
			request.Fields = "files(id, name, size, modifiedTime, mimeType,createdTime)";
			var response = await request.ExecuteAsync();
			return response.Files;
		}


		public static async Task<IList<Google.Apis.Drive.v3.Data.File>> GetFolders(string parentId)
		{
			var request = service.Files.List();
			request.Q = $"'{parentId}' in parents and mimeType = 'application/vnd.google-apps.folder'";
			request.Fields = "files(id, name, size, modifiedTime, mimeType,createdTime)";
			var response = await request.ExecuteAsync();
			return response.Files;
		}


		public static async Task CheckFileChanges(string driveFileId, string localFileDir)
		{

			var excistDriveItems = await GetFiles(driveFileId);
			List<string> localItems = Directory.GetFiles(localFileDir).ToList();


			foreach (var driveItem in excistDriveItems)
			{
				bool itemExcist = false;

				foreach (var localItem in localItems)
				{
					if (driveItem.Name == Path.GetFileName(localItem))
					{
						itemExcist = true;
						FileInfo fileInfo = new FileInfo(localItem);
						DateTime localTime = fileInfo.LastWriteTime;
						DateTime driveTime = driveItem.ModifiedTime.Value;

						if (driveItem.MimeType != "application/vnd.google-apps.folder" && Math.Abs((driveTime - localTime).TotalMinutes) > 5)
						{
							Console.WriteLine("Updating.. " + driveItem.Name + "  " + driveTime + "  " + localTime);
							await DeleteItem(driveItem.Id);

							break;
						}
					}
				}

				if (!itemExcist)
				{
					Console.WriteLine("Removing.. " + driveItem.Name);
					await DeleteItem(driveItem.Id);
				}
			}

		}

		public static async Task<string> CheckFolderChanges(string driveFileId, string localFileDir)
		{
			var excistDriveItems = await GetFolders(driveFileId);

			List<string> localDirections = Directory.GetDirectories(localFileDir).ToList();


			if (localDirections.Count == 0)
			{
				await CheckFileChanges(driveFileId, localFileDir);

			}




			foreach (var driveItem in excistDriveItems)
			{
				bool itemExcist = false;
				foreach (var localFile in localDirections)
				{
					//Console.WriteLine(driveItem.MimeType + "  " + localFile);					//Console.WriteLine(driveItem.MimeType + "  " + localFile);

					if (driveItem.Name == Path.GetFileName(localFile))
					{
						itemExcist = true;
						await CheckFolderChanges(driveItem.Id, localFile);
						break;
					}
				}

				if (!itemExcist && localDirections.Count != 0)
				{
					Console.WriteLine("Removing.. " + driveItem.Name);
					await DeleteItem(driveItem.Id);
				}

			}

			return "Check Complete";
		}


		public static async void TransferToFile()
		{
			Console.WriteLine("Syncing..");

			try
			{

				string json = System.IO.File.ReadAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"/drivesyncer.json");
				GoogleCredential credential = GoogleCredential.FromJson(json)
					.CreateScoped(DriveService.Scope.Drive);
				// Create Drive API service.
				service = new DriveService(new BaseClientService.Initializer
				{
					HttpClientInitializer = credential,
				});

				await CheckFolderChanges(DriveRootFolderId, FilePath);





				List<string> localFolders = Directory.GetDirectories(FilePath).ToList();

				var driveFolderList = await GetFolders(DriveRootFolderId);




				List<string> excistFolders = new List<string>();

				foreach (var localDir in localFolders)
				{
					bool fileExcist = false;
					foreach (var driveFile in driveFolderList)
					{
						if (driveFile.Name == Path.GetFileName(localDir))
						{
							fileExcist = true;

							var excistFileItems = await GetFiles(driveFile.Id);

							List<string> items = Directory.GetFiles(localDir).ToList();

							foreach (var item in items)
							{
								bool itemExcist = false;
								foreach (var driveItem in excistFileItems)
								{
									if (driveItem.Name == Path.GetFileName(item))
									{
										itemExcist = true;
										break;
									}
								}

								if (!itemExcist)
								{
									Console.WriteLine("Creating.. " + Path.GetFileName(item));

									var itemResponse = await UploadFile(item, "application/vnd.google-apps", driveFile.Id);
								}
							}

							break;
						}

					}

					if (!fileExcist)
						excistFolders.Add(localDir);
				}



				foreach (var folder in excistFolders)
				{

					var response = await CreateFile(folder, "application/vnd.google-apps.folder", DriveRootFolderId);

					List<string> items = Directory.GetFiles(folder).ToList();

					foreach (var item in items)
					{
						var itemresponse = await UploadFile(item, "application/vnd.google-apps", response.Id);
					}

				}

				Console.WriteLine("Senkronizasyon Başarili");


			}
			catch (Exception e)
			{

				if (e is AggregateException)
				{
					Console.WriteLine("Credential Not found");
				}
				else if (e is FileNotFoundException)
				{
					Console.WriteLine("File not found");
				}
				else
				{
					Console.WriteLine("Somethings not working and error not found");

					throw;
				}
			}

		}








	}
}
