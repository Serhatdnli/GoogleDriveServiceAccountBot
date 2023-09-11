# Türkçe DriveSyncer Kullanım Kılavuzu

Bu belge, DriveSyncer uygulamasını yapılandırma ve kullanma işlemleri hakkında adımları içermektedir.

## Setup.txt Dosyası Düzenleme

1. `Setup.txt` dosyasını bir metin düzenleyici ile açın.
2. Dosyayı aşağıdaki biçimde düzenleyin: `LocalKlasorAdi,DriveKlasorID`.
3. Değişiklikleri kaydedin.

## Dosya Yerleşimi

- `LocalKlasorAdi`, `Setup.txt` ve `drivesyncer.json` dosyalarının aynı konumda bulunması gerekmektedir.
- Bu dosyalar, `DriveSyncer.exe` ile aynı klasörde bulunmalıdır.

## Drive Klasör Kimliği (ID) Bulma

Drive Klasör Kimliği (ID) gerektiğinde aşağıdaki adımları takip edin:
1. https://drive.google.com/drive/folders/  "12pbpmOMKomiDYrSNgvAnXMw_wlvnxqOn" <-- Klasör ID'si.
2. Klasörün URL'sinde bulunan Klasör Kimliği (ID) not edin.

## Hizmet Hesabı Oluşturma

Hizmet Hesabı oluşturmak için aşağıdaki adımları takip edin:
1. [Google Cloud Console](https://console.cloud.google.com/) adresine gidin.
2. Yeni bir Google Drive API Projesi oluşturun.
3. Service Account (Hizmet Hesabı) oluşturun.
4. Oluşturulan e-posta hesabına ana Google Drive yetkilerini verin.
5. `credential.json` dosyasını indirin ve bu dosyayı `drivesyncer.json` adıyla `DriveSyncer.exe` ile aynı klasöre kaydedin.

## Google Drive Mime Türleri

Google Drive için desteklenen Mime Türleri hakkında daha fazla bilgi için [buraya](https://developers.google.com/drive/api/guides/mime-types?hl=tr) bakabilirsiniz.

## Drive Belgelerini Yönetme

Drive belgelerini yükleme ve yönetme konusunda daha fazla bilgi için aşağıdaki kaynaklara başvurabilirsiniz:
- [Yükleme İşlemleri](https://developers.google.com/drive/api/guides/manage-uploads?hl=tr#resumable)
- [Klasörlerle Çalışma](https://developers.google.com/drive/api/guides/folder?hl=tr)


# English DriveSyncer User Guide

This document contains steps for configuring and using the DriveSyncer application.

## Editing the Setup.txt File

1. Open the `Setup.txt` file with a text editor.
2. Edit the file in the following format: `LocalFolderName,DriveFolderID`.
3. Save the changes.

## File Placement

- `LocalFolderName`, `Setup.txt`, and `drivesyncer.json` files should be in the same location.
- These files should be in the same folder as `DriveSyncer.exe`.

## Finding Drive Folder ID

To find the Drive Folder ID when needed, follow these steps:
1. Last part of url is your folder id.
2. Sample: https://drive.google.com/drive/folders/    -->    12pbpmOMKomiDYrSNgvAnXMw_wlvnxqOn <-- your folder id.
3. Note the Folder ID found in the folder's URL.

## Creating a Service Account

To create a Service Account, follow these steps:
1. Go to [Google Cloud Console](https://console.cloud.google.com/).
2. Create a new Google Drive API Project.
3. Create a Service Account.
4. Grant the Service Account permissions to access your main Google Drive.
5. Download the `credential.json` file and save it in the same folder as `DriveSyncer.exe` with the name `drivesyncer.json`.

## Google Drive Mime Types

For more information about supported Mime Types for Google Drive, refer to [this link](https://developers.google.com/drive/api/guides/mime-types?hl=en).

## Managing Drive Documents

For information on uploading and managing Drive documents, refer to the following resources:
- [Upload Procedures](https://developers.google.com/drive/api/guides/manage-uploads?hl=en#resumable)
- [Working with Folders](https://developers.google.com/drive/api/guides/folder?hl=en)




