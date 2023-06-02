# Library-CSharp
ISBN Batch Query Program

## Description

This project is a program that reads a txt file containing ISBN codes of books. The program performs batch requests to an API using the ISBN codes found in the file. The information returned by the API is stored in a cache for later use.

If new ISBN codes are added to the txt file, the program checks if the codes are already present in the cache. If not, it makes a new request to the API for the codes that don't have stored information.

Upon finishing the file reading and validations, the program creates a csv file filled with information from both the cache and API responses. This csv file is saved in the "Documents" folder on the user's computer. It can be opened in a DataGridView within the program or in other programs that support the csv format.

The program's executable can be found in the "bin/release" folder.

## Key Features

- Reading ISBN codes from a txt file.
- Batch querying the API using the ISBN codes.
- Storing information in a cache to avoid redundant API requests.
- Validation of ISBN codes and separation into valid and invalid codes.
- Creation of a csv file with information from the cache and API.
- Saving the csv file in the "Documents" folder.
- Displaying the csv file in a DataGridView within the program.
- Compatibility with programs that support the csv format.

## Installation and Usage

1. Download the executable file from the "bin/release" folder.
2. Run the file to start the program.
3. Select the txt file containing the ISBN codes.
4. The program will perform the API queries and generate the csv file in the "Documents" folder.
5. Open the csv file within the program or in another compatible program.

*The program displays the csv in the datagridview at the end of the process.

## Contributions

Contributions to this project are welcome. If you encounter any issues, have suggestions, or want to add new features, feel free to create a pull request or open an issue in the repository.

## License
Open

## Author

This project was developed by Felipe Pérez Martínez.

- LinkedIn: [Felipe Pérez Martínez](https://www.linkedin.com/in/perezmfelipe/)


