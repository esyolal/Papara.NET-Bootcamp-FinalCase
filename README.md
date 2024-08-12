# Papara .NET Bootcamp Final Case

This repository contains the final project for the Papara .NET Bootcamp. It represents a simulated e-commerce system where users can manage their shopping cart, process payments, and handle user authentication. The project leverages a variety of modern .NET technologies and architectural patterns to create a robust and scalable application.

## Project Overview

The project is designed to provide the following functionalities:

- **User Authentication**: Register and log in users with JWT (JSON Web Tokens) for secure authentication.
- **Cart Management**: Add and remove products from the user's cart, and view the cart's contents.
- **Payment Processing**: Complete purchases using a fake credit card system, integrated with the user's wallet.
- **Coupons and Points**: Apply discounts via coupons and use points during the checkout process.

## Technologies Used

- **.NET 8.0**: The core framework for building the application, providing the foundation for the entire system.
- **CQRS (Command Query Responsibility Segregation)**: An architectural pattern to separate read and write operations, enhancing scalability and maintainability.
- **MediatR**: A library used to implement CQRS, facilitating communication between different parts of the application.
- **JWT (JSON Web Tokens)**: Used for secure user authentication and authorization.
- **AutoMapper**: Automates the mapping between domain models and DTOs (Data Transfer Objects).
- **FluentValidation**: Provides a robust mechanism for validating user input and commands.
- **Entity Framework Core**: ORM (Object-Relational Mapper) for database interactions and migrations.
- **Unit of Work**: Manages database transactions to ensure consistency and handle multiple repository operations.
- **Generic Repository**: Offers a generic implementation for common data access operations, simplifying CRUD operations.

## Getting Started

Follow these steps to set up and run the project locally:

### Prerequisites

- **.NET 8.0 SDK**: Ensure you have .NET 8.0 or higher installed. You can download it from the [.NET official website](https://dotnet.microsoft.com/download).
- **SQL Server**: The project uses SQL Server for data storage. Make sure you have a compatible database server set up.
- **Postman** or another API testing tool for interacting with the API endpoints.

### Installation

1. **Clone the Repository**

   Clone the repository to your local machine using Git:

   `git clone https://github.com/esyolal/Papara.NET-Bootcamp-FinalCase.git`

   Change directory to the project folder:

   `cd Papara.NET-Bootcamp-FinalCase`

2. **Install Dependencies**

   Restore the required NuGet packages for the project:

   `dotnet restore`

3. **Configure Database**

   Update the connection string in `appsettings.json` to match your database configuration. This file is located in the `src` directory of the project:

   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=YourDatabase;User Id=yourusername;Password=yourpassword;"
     }
   }

4. **Apply Database Migrations**

   Run the Entity Framework Core migrations to create and update the database schema:

   `dotnet ef database update`

5. **Run the Application**

   Start the application using the following command:

   `dotnet run`

   By default, the application will be available at `http://localhost:5000`.

## API Usage

### Users

- **Register**: `POST /api/users/register`
  - Registers a new user.
  - **Request Body**: JSON object with user details (e.g., `username`, `password`).

- **Login**: `POST /api/users/login`
  - Authenticates a user and returns a JWT token.
  - **Request Body**: JSON object with login credentials (e.g., `username`, `password`).

### Cart

- **Add Product**: `POST /api/cart/add`
  - Adds a product to the cart.
  - **Request Body**: JSON object with product ID and quantity.

- **Remove Product**: `POST /api/cart/remove`
  - Removes a product from the cart.
  - **Request Body**: JSON object with product ID.

- **View Cart**: `GET /api/cart`
  - Retrieves the current contents of the user's cart.

### Payment

- **Checkout**: `POST /api/cart/checkout`
  - Processes the payment for the items in the cart.
  - **Request Body**: JSON object with payment details.

### Credit Card

- **Create Fake Credit Card**: `POST /api/wallet/generate-credit-card`
  - Generates a fake credit card for testing purposes.
  - **Request Body**: Optionally, specify parameters if required.

## Running Tests

To run the unit tests and ensure everything is functioning correctly:

`dotnet test`

## Contributing

To contribute to this project:

1. **Create a Feature Branch**

   `git checkout -b feature/your-feature`

2. **Make Changes**

   Implement your feature or bug fix, and ensure it works as expected.

3. **Keep Your Branch Updated**

   `git fetch origin`
   `git rebase origin/main`

4. **Commit and Push Changes**

   `git add .`
   `git commit -m "Add feature or fix bug"`
   `git push origin feature/your-feature`

5. **Submit a Pull Request**

   Go to the repository on GitHub and create a pull request from your feature branch.

## License

This project is licensed under the [MIT License](LICENSE).

## Contact

For questions or additional information:

- **Email**: esyolal@gmail.com
- **GitHub**: [esyolal](https://github.com/esyolal)
