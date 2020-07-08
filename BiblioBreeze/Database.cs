using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Upload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiblioBreeze
{
    public class Database
    {
        public static Database db;

        public enum Source { Teachers, Books, Students, ReadingData};
        public Dictionary<Source, string> sources = new Dictionary<Source, string>()
        {
            { Source.Teachers, "Teachers" },
            { Source.Students, "Students" },
            { Source.ReadingData, "Reading Data" },
            { Source.Books, "Books" }
        };

        private string[] _scopes = { SheetsService.Scope.Spreadsheets, DriveService.Scope.Drive}; // Change this if you're accessing Drive or Docs
        private string _applicationName = "BiblioBreeze ";
        private string _spreadsheetId = "14zOMeKwzGeWpqTJoj_yBiIgpEO9ai0nKI_roBuraUjo";
        private SheetsService _sheetsService;
        private DriveService _driveService;

        public Database()
        {
            ConnectToGoogle();
            db = this;
        }

        private void ConnectToGoogle()
        {
            GoogleCredential credential;

            // Put your credentials json file in the root of the solution and make sure copy to output dir property is set to always copy 
            using (var stream = new FileStream("credentials.json",
                FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(_scopes);
            }

            // Create Google Sheets API service.
            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName
            });

            // Create Google Drive API service.
            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName
            });
        }

        public string UploadToDrive(string path, string name, Action<IUploadProgress> progressUpdates)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = name,
                MimeType = "application/epub+zip",
                Parents = new List<string> { "1h_2TUIxmsUHLf9AKR9oGBFYAxAWjjf2p" }
            };

            FilesResource.CreateMediaUpload request;

            using (var stream = new System.IO.FileStream(path,
                                    System.IO.FileMode.Open))
            {
                request = _driveService.Files.Create(fileMetadata, stream, "application/epub+zip");

                request.Fields = "id";
                request.ProgressChanged += progressUpdates;

                request.Upload();
            }
            
            return request.ResponseBody.Id;
        }

        public async Task<string> UploadToDriveAsync(string path, string name, Action<IUploadProgress> progressUpdates)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = name,
                MimeType = "application/epub+zip",
                Parents = new List<string> { "1h_2TUIxmsUHLf9AKR9oGBFYAxAWjjf2p" }
            };

            FilesResource.CreateMediaUpload request;

            using (var stream = new System.IO.FileStream(path,
                                    System.IO.FileMode.Open))
            {
                request = _driveService.Files.Create(fileMetadata, stream, "application/epub+zip");

                request.Fields = "id";
                request.ProgressChanged += progressUpdates;

                await request.UploadAsync();
                return request.ResponseBody.Id;
            }
        }

        public void DownloadFromDrive(string id, Action<IDownloadProgress> progressUpdates)
        {
            var request = _driveService.Files.Get(id);
            var stream = new System.IO.MemoryStream();

            request.MediaDownloader.ProgressChanged += progressUpdates;

            request.MediaDownloader.ProgressChanged +=
                (IDownloadProgress progress) =>
                    {
                        if(progress.Status == DownloadStatus.Completed)
                        {
                            SaveStream(stream, id);
                        }
                    };

            request.Download(stream);
        }

        public async Task DownloadFromDriveAsync(string id, Action<IDownloadProgress> progressUpdates)
        {
            var request = _driveService.Files.Get(id);
            var stream = new System.IO.MemoryStream();

            request.MediaDownloader.ProgressChanged += progressUpdates;

            request.MediaDownloader.ProgressChanged +=
                (IDownloadProgress progress) =>
                {
                    if (progress.Status == DownloadStatus.Completed)
                    {
                        SaveStream(stream, id);
                    }
                };

            await request.DownloadAsync(stream);
        }

        // Pass in your data as a list of a list (2-D lists are equivalent to the 2-D spreadsheet structure)
        public void UpdateData(List<IList<object>> data)
        {
            String range = "Sheet1!A1:Y";
            string valueInputOption = "USER_ENTERED";

            // The new values to apply to the spreadsheet.
            List<Google.Apis.Sheets.v4.Data.ValueRange> updateData = new List<Google.Apis.Sheets.v4.Data.ValueRange>();
            var dataValueRange = new Google.Apis.Sheets.v4.Data.ValueRange();
            dataValueRange.Range = range;
            dataValueRange.Values = data;
            updateData.Add(dataValueRange);

            Google.Apis.Sheets.v4.Data.BatchUpdateValuesRequest requestBody = new Google.Apis.Sheets.v4.Data.BatchUpdateValuesRequest();
            requestBody.ValueInputOption = valueInputOption;
            requestBody.Data = updateData;

            var request = _sheetsService.Spreadsheets.Values.BatchUpdate(requestBody, _spreadsheetId);

            var response = request.Execute();
            // Data.BatchUpdateValuesResponse response = await request.ExecuteAsync(); // For async         
        }

        public void WriteToCell(int col, int row, string val, Source sheet)
        {
            string range = String.Format("{0}!{1}{2}", sources[sheet], IntToLetters(col), row);
            ValueRange valueRange = new ValueRange();
            valueRange.MajorDimension = "COLUMNS";//"ROWS";//COLUMNS

            var oblist = new List<object>() { val };
            valueRange.Values = new List<IList<object>> { oblist };

            SpreadsheetsResource.ValuesResource.UpdateRequest update = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
            update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            var response = update.Execute();
        }

        public string ReadCell(int col, int row, Source sheet)
        {
            string range = String.Format("{0}!{1}{2}", sources[sheet], IntToLetters(col), row);

            SpreadsheetsResource.ValuesResource.GetRequest request =
                    _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);

            ValueRange response = request.Execute();

            return response.Values[0][0].ToString();
        }

        public List<int> FindRowsByColVal(int column, string val, Source sheet)
        {
            ValueRange response = _sheetsService.Spreadsheets.
                Values.Get(_spreadsheetId, String.Format("{0}!{1}:{1}", sources[sheet], IntToLetters(column))).Execute();

            List<object> colValues = response.Values.Select(x => x[0]).ToList();
            List<int> rowIndices = new List<int>();

            if (colValues != null)
            {
                for (int i = 0; i < colValues.Count; i++)
                {
                    if ((string)colValues[i] == val)
                    {
                        rowIndices.Add(i + 1);
                    }
                }
            }

            return rowIndices;
        }

        public string IntToLetters(int value)
        {
            string result = string.Empty;
            while (--value >= 0)
            {
                result = (char)('A' + value % 26) + result;
                value /= 26;
            }
            return result;
        }

        public List<string> ReadRow(int row, Source sheet)
        {
            string range = String.Format("{0}!{1}:{1}", sources[sheet], row);

            SpreadsheetsResource.ValuesResource.GetRequest request =
                    _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);

            ValueRange response = request.Execute();

            return response.Values[0].Select(x => x as string).ToList();
        }

        public void WriteRow(int row, List<object> values, Source sheet)
        {
            string range = String.Format("{0}!{1}:{1}", sources[sheet], row);

            ValueRange valueRange = new ValueRange();
            valueRange.MajorDimension = "ROWS";//"ROWS";//COLUMNS

            valueRange.Values = new List<IList<object>> { values };

            SpreadsheetsResource.ValuesResource.UpdateRequest update = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
            update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            var response = update.Execute();
        }

        public void AppendRow(List<object> values, Source sheet)
        {
            string range = String.Format("{0}!A:A", sources[sheet]);
            
            ValueRange valueRange = new ValueRange();
            valueRange.MajorDimension = "ROWS";//"ROWS";//COLUMNS

            valueRange.Values = new List<IList<object>> { values };

            SpreadsheetsResource.ValuesResource.AppendRequest request = _sheetsService.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

            request.Execute();
        }

        public void DeleteRow(int row, Source sheet)
        {
            Request RequestBody = new Request()
            {
                DeleteDimension = new DeleteDimensionRequest()
                {
                    Range = new DimensionRange()
                    {
                        SheetId = 2,
                        Dimension = "ROWS",
                        StartIndex = row,
                        EndIndex = row + 1
                    }
                }
            };

            List<Request> RequestContainer = new List<Request>();
            RequestContainer.Add(RequestBody);

            BatchUpdateSpreadsheetRequest DeleteRequest = new BatchUpdateSpreadsheetRequest();
            DeleteRequest.Requests = RequestContainer;

            SpreadsheetsResource.BatchUpdateRequest Deletion = new SpreadsheetsResource.BatchUpdateRequest(_driveService, DeleteRequest, _spreadsheetId);
            Deletion.Execute();
        }

        public string FindLastColVal(int col, Source sheet)
        {
            string range = String.Format("{0}!{1}:{1}", sources[sheet], IntToLetters(col));

            SpreadsheetsResource.ValuesResource.GetRequest request =
                    _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);

            ValueRange response = request.Execute();

            int lastRow = response.Values.Count();
            return ReadCell(col, lastRow, sheet);
        }

        private void SaveStream(System.IO.MemoryStream stream, string saveTo)
        {
            using (System.IO.FileStream file = new System.IO.FileStream(Path.Combine("Library", saveTo) + ".epub", System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                stream.WriteTo(file);
            }
        }
    }
}
