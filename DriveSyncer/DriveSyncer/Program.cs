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
using static System.Net.WebRequestMethods;

namespace DriveSyncer
{
	class Program
	{
		static string FilePath;
		static string DriveRootFolderId;

		static string setupPath = @"\Setup.txt";

		static void Main(string[] args)
		{

			string[] setup = System.IO.File.ReadAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + setupPath).Split(',');
			FilePath = setup[0];
			DriveRootFolderId = setup[1];

			TransferToFile();
			Console.ReadKey();

		}


		public static async Task<string> DeleteItem(DriveService service, string itemId)
		{
			var response = await service.Files.Delete(itemId).ExecuteAsync();
			return response;
		}

		//public static List<string> GetFolderNames() => Directory.GetDirectories(FilePath).ToList();
		public static async Task<Google.Apis.Upload.IUploadProgress> UploadFile(DriveService service, string filePath, string mimeType, string parentId)
		{
			var excistItemMetadata = new Google.Apis.Drive.v3.Data.File()
			{
				MimeType = mimeType,
				Name = Path.GetFileName(filePath),
				Parents = new List<string>
				{
					parentId
				}
			};

			using (var stream = new FileStream(filePath, FileMode.Open))
			{
				var itemrequest = service.Files.Create(body: excistItemMetadata, stream, mimeType);
				var itemresponse = await itemrequest.UploadAsync();	


				return itemresponse;


			}

		}

		public static async Task<Google.Apis.Drive.v3.Data.File> CreateFile(DriveService service, string filePath, string mimeType, string parentId)
		{
			var excistItemMetadata = new Google.Apis.Drive.v3.Data.File()
			{
				MimeType = mimeType,
				Name = Path.GetFileName(filePath),
				Parents = new List<string>
				{
					parentId
				}

			};


			var itemrequest = service.Files.Create(body: excistItemMetadata);
			var itemresponse = await itemrequest.ExecuteAsync();



			return itemresponse;

		}


		public static async Task<IList<Google.Apis.Drive.v3.Data.File>> GetFiles(DriveService service, string parentId)
		{
			var request = service.Files.List();
			request.Q = $"'{parentId}' in parents";
			var response = await request.ExecuteAsync();
			return response.Files;
		}

		public static async Task<string> DeleteNotExcistFolders(DriveService service, string fileId, string dirFile)
		{
			var excistDriveItems = await GetFiles(service, fileId);

			List<string> items = Directory.GetDirectories(dirFile).ToList();

			foreach (var driveItem in excistDriveItems)
			{
				bool itemExcist = false;
				foreach (var item in items)
				{

					if (driveItem.Name == Path.GetFileName(item))
					{
						itemExcist = true;

						await DeleteNotExcistFolders(service, driveItem.Id, item);

					}

				}

				if (!itemExcist && items.Count != 0)
				{
					Console.WriteLine("Removing.. " + driveItem.Name);
					await DeleteItem(service, driveItem.Id);
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
				var service = new DriveService(new BaseClientService.Initializer
				{
					HttpClientInitializer = credential,
				});




				List<string> dirNames = Directory.GetDirectories(FilePath).ToList();

				var fileList = await GetFiles(service, DriveRootFolderId);


				await DeleteNotExcistFolders(service, DriveRootFolderId, FilePath);

				//foreach (var file in fileList)
				//{
				//	bool fileExcist = false;
				//	foreach (var dirFile in dirNames)
				//	{
				//		if (file.Name == Path.GetFileName(dirFile))
				//		{
				//			fileExcist = true;

				//			var excistDriveItems = await GetFiles(service, file.Id);

				//			List<string> items = Directory.GetFiles(dirFile).ToList();

				//			foreach (var driveItem in excistDriveItems)
				//			{
				//				bool itemExcist = false;
				//				foreach (var item in items)

				//				{
				//					if (driveItem.Name == Path.GetFileName(item))
				//					{
				//						itemExcist = true;
				//						break;
				//					}
				//				}

				//				if (!itemExcist)
				//				{
				//					await DeleteItem(service, driveItem.Id);
				//				}
				//			}

				//			break;
				//		}
				//	}

				//	if (!fileExcist)
				//	{
				//		await DeleteItem(service, file.Id);
				//	}
				//}

				List<string> excistFolders = new List<string>();

				foreach (var dirFile in dirNames)
				{
					bool fileExcist = false;
					foreach (var file in fileList)
					{
						if (file.Name == Path.GetFileName(dirFile))
						{
							fileExcist = true;

							var excistFileItems = await GetFiles(service, file.Id);

							List<string> items = Directory.GetFiles(dirFile).ToList();

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
									var itemResponse = await UploadFile(service, item, "application/vnd.google-apps", file.Id);
								}
							}

							break;
						}

					}

					if (!fileExcist)
						excistFolders.Add(dirFile);
				}

				foreach (var folder in excistFolders)
				{
					//var excistFileMetadata = new Google.Apis.Drive.v3.Data.File()
					//{
					//	Name = Path.GetFileName(folder),
					//	MimeType = "application/vnd.google-apps.folder",
					//	Parents = new List<string>
					//	{
					//		DriveRootFolderId
					//	}
					//};

					//var request = service.Files.Create(body: excistFileMetadata);
					//var response = await request.ExecuteAsync();

					var response = await CreateFile(service, folder, "application/vnd.google-apps.folder", DriveRootFolderId);

					List<string> items = Directory.GetFiles(folder).ToList();

					foreach (var item in items)
					{
						//var excistItemMetadata = new Google.Apis.Drive.v3.Data.File()
						//{
						//	Name = Path.GetFileName(item),
						//	MimeType = "application/vnd.google-apps",
						//	Parents = new List<string>
						//	{
						//		response.Id
						//	}
						//};

						//using (var stream = new FileStream(item, FileMode.Open))
						//{
						//	var itemrequest = service.Files.Create(body: excistItemMetadata, stream, "application/vnd.google-apps");
						//	var itemresponse = await itemrequest.UploadAsync();

						//}

						var itemresponse = await UploadFile(service, item, "application/vnd.google-apps", response.Id);

					}





					//var file = response.Id;


					//daha sonra içindekileri upload etme.
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
