# Papara .NET Bootcamp Final Case

An ASP.NET Core API for a simulated e-commerce checkout. Users can browse the catalog, manage a cart, fund a wallet with a test card, apply a coupon or reward points, and view their order history. Administrators manage users, products, categories, and coupons.

## Run locally

You need the .NET 8 SDK, PowerShell, and Docker Desktop running Linux containers. Run these commands from the repository root:

```powershell
dotnet restore PaparaFinalCase.sln
powershell.exe -NoProfile -ExecutionPolicy RemoteSigned -File .\scripts\setup-local-sql.ps1
```

The setup script starts SQL Server 2022 Developer Edition on `127.0.0.1:14333`, creates the `PaparaFinalCase` database, applies EF Core migrations, and saves the connection in the API's User Secrets. Running the script accepts the SQL Server license terms. The `papara-sql-data` Docker volume retains the database.

If you already have a SQL Server instance, set `ConnectionStrings:MsSqlConnection` in **Pp.Api → Manage User Secrets** in Visual Studio, then run `dotnet run --project Pp.Api -- --migrate`. Development generates a temporary JWT signing key when `JwtConfig:Secret` is empty. Set a random secret of at least 32 bytes in User Secrets if tokens need to survive restarts. Production requires an explicit key, which can be supplied through `JwtConfig__Secret`.

### Create the first administrator

From the repository root, run the following in PowerShell. Use a password of at least 12 characters and no more than 72 UTF-8 bytes. The command stops if an administrator or an account with the same email already exists.

```powershell
$env:AdminSetup__Email = 'admin@local.test'
$adminPassword = Read-Host 'Administrator password' -AsSecureString
$env:AdminSetup__Password = [pscredential]::new('admin', $adminPassword).GetNetworkCredential().Password

try {
    dotnet run --project Pp.Api -- --create-admin
} finally {
    Remove-Item Env:AdminSetup__Email, Env:AdminSetup__Password
}
```

### Start the API

```powershell
dotnet run --project Pp.Api --launch-profile https
```

Open [Swagger](https://localhost:7219/swagger/index.html). If you run the API with Visual Studio's IIS Express profile, open [Swagger on port 44339](https://localhost:44339/swagger/index.html).

## Try a checkout in Swagger

1. Sign in with `POST /api/User/login` using `email` and `password`. Paste the returned `data.token` into **Authorize**. Swagger adds the `Bearer` prefix.
2. As an administrator, create a category with `POST /api/Category` and use the returned `data.id` in the `categoryIds` array when calling `POST /api/Product`. Set `isActive` to `true` so shoppers can add the product to a cart. Optionally create a coupon with `POST /api/Coupon` using `code`, `discountAmount`, `startsAt`, and `endsAt`.
3. Register a shopper with `POST /api/User/register`, then sign in and replace the administrator token with the shopper token.
4. Call `POST /api/Wallet/generate-credit-card`. Use the returned card details and an `amount` in `POST /api/Wallet/add-funds`. Check the wallet with `GET /api/Wallet/wallet`. Test cards and their remaining balances reset when the API restarts.
5. Product responses omit IDs. Look up the new product in the local `PaparaFinalCase` database with `SELECT Id, Name FROM dbo.Products;`. Use that ID as `productId` with `quantity` in `POST /api/Cart/add`. Get the cart ID from `GET /api/Cart`.
6. Send `cartId`, an optional `couponCode`, and `usePoints` to `POST /api/Cart/checkout`. View the purchase with `GET /api/Order`.

A coupon applies a fixed discount and can be redeemed once. One reward point offsets one unit of currency. Products can earn points on the amount paid from the wallet, up to their configured reward limit. Check `isSuccess` in checkout and wallet responses for business-rule failures.

Administrators can list coupons with `GET /api/Coupon` and deactivate a code with `DELETE /api/Coupon/{code}`.

## Project structure

- `Pp.Api` contains the controllers, JWT configuration, Swagger, and local setup commands.
- `Pp.Business` contains MediatR handlers and checkout rules.
- `Pp.Data` contains the EF Core model, repositories, and SQL Server migrations.
- `Pp.Schema` and `Pp.Base` contain request and response models and shared types.
- `Pp.Tests` contains NUnit tests for account handling, carts, checkout, test cards, and migration SQL generation.

## Run tests

```powershell
dotnet test PaparaFinalCase.sln
```

The tests use mocked repositories and generate migration SQL without connecting to a running database.

## License

[MIT](LICENSE)
