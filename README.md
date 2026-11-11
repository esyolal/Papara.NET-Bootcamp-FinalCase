# Papara .NET Bootcamp Final Case

This repository contains the final project for the Papara .NET Bootcamp. It represents a simulated e-commerce system where users can manage their shopping cart, process payments, and handle user authentication.

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

3. **Configure SQL Server**

   In Visual Studio, right-click `Pp.Api` and select **Manage User Secrets**. Set `ConnectionStrings:MsSqlConnection` to your local or test SQL Server connection string. Set `JwtConfig:Secret` to a random secret of at least 32 bytes to keep tokens valid across restarts. Development generates a temporary key when this setting is empty. Production requires an explicit secret. The corresponding environment variables are `ConnectionStrings__MsSqlConnection` and `JwtConfig__Secret`.

4. **Apply the database migrations**

   Run the database setup command from the repository root:

   ```powershell
   dotnet run --project Pp.Api -- --migrate
   ```

5. **Create the first administrator**

   Set `AdminSetup__Email` and `AdminSetup__Password` as environment variables in your terminal. Use a password of at least 12 characters and at most 72 UTF-8 bytes. Run the command below, then clear the two variables from the terminal.

   ```powershell
   dotnet run --project Pp.Api -- --create-admin
   ```

6. **Start the API**

   ```powershell
   dotnet run --project Pp.Api --launch-profile https
   ```

   Open `https://localhost:7219/swagger`. Visual Studio's IIS Express profile uses `https://localhost:44339/swagger`.

## API Usage

### Local SQL Server with Docker

With Docker Desktop running, open PowerShell in the repository root and run:

```powershell
powershell.exe -NoProfile -ExecutionPolicy RemoteSigned -File .\scripts\setup-local-sql.ps1
```

The script starts SQL Server 2022 Developer Edition on `127.0.0.1:14333`, applies the migrations, and saves the connection in User Secrets. Running it accepts the SQL Server Developer license terms. The `papara-sql-data` Docker volume retains the database.

### Users

- **Register**: `POST /api/User/register`
  - Registers a new user.
  - **Request Body**: JSON object with user details (`name`, `surname`, `email`, `password` and `phone`).

- **Login**: `POST /api/User/login`
  - Authenticates a user and returns a JWT token.
  - **Request Body**: JSON object with `email` and `password`.

### Cart

- **Add Product**: `POST /api/cart/add`
  - Adds a product to the cart.
  - **Request Body**: JSON object with product ID and quantity.

- **Remove Product**: `POST /api/cart/remove`
  - Removes a product from the cart.
  - **Request Body**: JSON object with product ID and quantity.

- **View Cart**: `GET /api/cart`
  - Retrieves the current contents of the user's cart.

### Payment

- **Checkout**: `POST /api/cart/checkout`
  - Processes the payment for the items in the cart.
  - **Request Body**: JSON object with `cartId`, optional `couponCode`, and `usePoints`. A coupon is a fixed discount and can be redeemed once. One point offsets one unit of currency. Product rewards are calculated on the amount paid from the wallet and capped by the product reward limit.

### Credit Card

- **Create Fake Credit Card**: `POST /api/wallet/generate-credit-card`
  - Generates a fake credit card for testing purposes.

### Coupons and orders

Administrators can list and create coupons with `GET /api/Coupon` and `POST /api/Coupon`, and deactivate a coupon with `DELETE /api/Coupon/{code}`. Users can view their own purchases with `GET /api/Order`.

Test cards and their remaining balances are stored in memory and expire when the API restarts.

## Running Tests

To run the unit tests and ensure everything is functioning correctly:

`dotnet test PaparaFinalCase.sln`

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
