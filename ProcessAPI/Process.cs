using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Proceso
{
    class Process
    {
     
        // Requests book information based on the provided file path
        public async Task<(List<string> InvalidIsbns, string Message)> Requests(string file)
        {
            // Define input and output file paths
            string inputFile = file;
            string outputFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "output.csv");

            // Load cached books into a dictionary
            Dictionary<string, BookInfo> cachedBooks = LoadCachedBooks();

            // Initialize lists for storing ISBNs
            List<string> isbnsToQuery = new List<string>();
            List<string> invalidIsbns = new List<string>();

            // Read the input file line by line
            using (StreamReader reader = new StreamReader(inputFile))
            {
                string line;
                int lineNumber = 1;
                while ((line = reader.ReadLine()) != null)
                {
                    // Split each line into an array of ISBNs
                    string[] isbnArray = line.Split(',');

                    // Iterate over each ISBN in the array
                    foreach (string isbn in isbnArray)
                    {
                        // Validate the ISBN
                        if (ValidateISBN(isbn))
                        {
                            // Add valid ISBNs to the list for querying
                            isbnsToQuery.Add(isbn);
                        }
                        else
                        {
                            // Add invalid ISBNs to the list
                            invalidIsbns.Add(isbn);
                        }
                    }
                    lineNumber++;
                }
            }

            // Create a new StreamWriter for writing to the output file
            using (StreamWriter writer = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                // Write the header line to the CSV file
                writer.WriteLine("Row Number;Data Retrieval Type;ISBN;Title;Subtitle;Author(s);Number of Pages;Publication Date");

                // Create a list for storing ISBNs not present in the cache
                List<string> isbnsNotInCache = new List<string>();

                int rowNumber = 1;
                // Iterate over each ISBN to check if it is present in the cache or needs to be retrieved from the server
                foreach (string isbn in isbnsToQuery)
                {
                    if (cachedBooks.ContainsKey(isbn))
                    {
                        // Retrieve the book information from the cache and write it to the CSV file
                        BookInfo bookInfo = cachedBooks[isbn];
                        await WriteBookInfoToCsv(writer, rowNumber, "Cache", isbn, bookInfo);
                    }
                    else
                    {
                        // Add the ISBN to the list of ISBNs not present in the cache
                        isbnsNotInCache.Add(isbn);
                    }
                    rowNumber++;
                }

                // Check if there are ISBNs that need to be retrieved from the server
                if (isbnsNotInCache.Count > 0)
                {
                    // Retrieve book information from the server for the ISBNs not present in the cache
                    List<BookInfo> bookInfos = await RetrieveBookInfoFromAPI(isbnsNotInCache);

                    // Create dictionaries for storing retrieved books and their corresponding ISBNs
                    Dictionary<string, BookInfo> retrievedBooks = new Dictionary<string, BookInfo>();

                    // Iterate over each retrieved book information
                    for (int i = 0; i < bookInfos.Count; i++)
                    {
                        string isbn = isbnsNotInCache[i];
                        BookInfo bookInfo = bookInfos[i];

                        // Check if book information was retrieved successfully
                        if (bookInfo != null)
                        {
                            // Update the cache and write the book information to the CSV file
                            cachedBooks[isbn] = bookInfo;
                            retrievedBooks[isbn] = bookInfo;
                            await WriteBookInfoToCsv(writer, rowNumber, "Server", isbn, bookInfo);
                        }
                        rowNumber++;
                    }

                    // Save the retrieved books to the cache
                    SaveCachedBooks(retrievedBooks);
                }
            }

            // Set the completion message and return the lists of invalid ISBNs and the completion message
            string message = "The process is finished. The CSV file with the book information has been generated. \nYou can find the file in the documents folder.";

            return (invalidIsbns, message);
        }



        private string WrapField(string field)
        {
            return $"\"{field}\"";
        }


        // Writes the book information to a CSV file using the provided StreamWriter
        private async Task WriteBookInfoToCsv(StreamWriter writer, int rowNumber, string operationType, string isbn, BookInfo bookInfo)
        {
            try
            {
                // Extract the authors' JSON data from the bookInfo object
                string authorsJson = bookInfo.Authors;

                // Construct a JSON string with the authors data
                string json = "{ \"authors\": [" + authorsJson + "]}";

                // Parse the JSON string into a JObject
                JObject authorsObject = JObject.Parse(json);

                // Retrieve the authors array from the JObject
                JArray authorsArray = (JArray)authorsObject["authors"];

                // Create a list to store the author names
                List<string> authorNames = new List<string>();

                // Iterate over each author object in the authors array
                foreach (JObject authorObject in authorsArray)
                {
                    // Extract the name of the current author
                    string authorName = authorObject["name"].ToString();

                    // Add the author name to the list
                    authorNames.Add(authorName);
                }

                // Join the author names into a single string separated by commas
                string authorsString = string.Join(", ", authorNames);

                string numberPage = (bookInfo.NumberOfPages > 0) ? bookInfo.NumberOfPages.ToString() : "N/A";

                string subtitle = !string.IsNullOrEmpty(bookInfo.Subtitle) ? bookInfo.Subtitle : "N/A";

                // Write the book information to the CSV file using the provided StreamWriter
                await writer.WriteLineAsync($"{rowNumber};{operationType};{isbn};{WrapField(bookInfo.Title)};{WrapField(subtitle)};{authorsString};{numberPage};{bookInfo.PublishDate}");
            }
            catch (Exception ex)
            {
                // Handle the exception, you can display an error message, log the error, or take some specific action based on your needs
                MessageBox.Show($"Error writing to CSV file: {ex.Message}");
            }
        }


        // Retrieves book information from the API for the given list of ISBNs
        static async Task<List<BookInfo>> RetrieveBookInfoFromAPI(List<string> isbns)
        {
            // Construct the API URL using the provided ISBNs
            string apiUrl = $"https://openlibrary.org/api/books?bibkeys=ISBN:{string.Join(",ISBN:", isbns)}&format=json&jscmd=data";
           

            // Create an HttpClient to send the API request
            using (HttpClient client = new HttpClient())
            {
                // Send a GET request to the API URL
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a string
                    var json = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON response into a dynamic object
                    var bookData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);

                    // Create a list to store the retrieved book information
                    List<BookInfo> bookInfos = new List<BookInfo>();

                    // Iterate over each ISBN in the input list
                    foreach (string isbn in isbns)
                    {
                        // Retrieve the book information for the current ISBN from the dynamic object
                        var bookInfo = bookData[$"ISBN:{isbn}"];

                        // Extract the individual properties from the bookInfo object
                        string title = bookInfo.title;
                        string subtitle = bookInfo.subtitle;
                        string authors = string.Join(", ", bookInfo.authors);
                        int? numberOfPagesNullable = bookInfo.number_of_pages;  // nullable int for number_of_pages
                        int numberOfPages = numberOfPagesNullable ?? 0;  // set number_of_pages to 0 if it's null
                        string publishDate = bookInfo.publish_date;

                        // Create a BookInfo object with the retrieved information and add it to the list
                        bookInfos.Add(new BookInfo(title, subtitle, authors, numberOfPages, publishDate));
                    }

                    // Return the list of book information
                    return bookInfos;
                }
                else
                {
                    
                    MessageBox.Show($"Error querying the API for the ISBNs. Response status: {response.StatusCode}");
                    return null;
                }
            }
        }



        static Dictionary<string, BookInfo> LoadCachedBooks()
        {
            string cacheFile = "cache.json";
            
            if (File.Exists(cacheFile))
            {
                string json = File.ReadAllText(cacheFile);
                return JsonConvert.DeserializeObject<Dictionary<string, BookInfo>>(json);
            }
            else
            {
                return new Dictionary<string, BookInfo>();
            }
        }

        //This method is responsible for saving the information in cache.
        static void SaveCachedBooks(Dictionary<string, BookInfo> cachedBooks)
        {
            string cacheFile = "cache.json";

            string json = JsonConvert.SerializeObject(cachedBooks, Formatting.Indented);
            File.WriteAllText(cacheFile, json);
        }

        //This method validates whether a code complies with the ISBN-10 or ISBN-13 standard.
        public bool ValidateISBN(string isbn)
        {
            // Validar ISBN-10
            if (isbn.Length == 10 && Regex.IsMatch(isbn, @"^\d{9}[\d|X]$"))
            {
                return true;
            }

            // Validar ISBN-13
            if (isbn.Length == 13 && Regex.IsMatch(isbn, @"^\d{13}$"))
            {
                return true;
            }

            return false;
        }

        //This method loads the CSV information and adds it to a datatable.
        public DataTable LoadCsvToDataTable(string filePath)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string[] headers = reader.ReadLine()?.Split(';');

                    if (headers != null)
                    {
                        foreach (string header in headers)
                        {
                            //Adding column names
                            dataTable.Columns.Add(header);
                        }

                        while (!reader.EndOfStream)
                        {
                            string[] rows = reader.ReadLine()?.Split(';');
                            dataTable.Rows.Add(rows);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar la excepción de acuerdo a tus necesidades
               MessageBox.Show("Error loading CSV file: " + ex.Message);
            }

            return dataTable;
        }
    }

}
