# NSales

NSales is a microservice responsible for **product registration** and
**sales order management**.\
It provides a simple and scalable API for handling product information,
creating and managing orders, and integrating with other backoffice
services.

------------------------------------------------------------------------

## 🚀 Features

-   **Product Registration**\
    Create, update, and retrieve product information.

-   **Sales Order Management**\
    Create, view, and update sales orders.

-   **Integration with Other Services**\
    Easily integrates with inventory, billing, or payment systems.

-   **RESTful API**\
    Standardized interface for seamless communication with other
    microservices.

------------------------------------------------------------------------

## 🛠️ Technologies

-   **.NET Core 8** for API development\
-   **Entity Framework Core** for data access\
-   **PostgreSQL** as the main database\
-   **Docker** for containerization and easy deployment\
-   **Swagger** for API documentation

------------------------------------------------------------------------

## 📦 Project Structure

``` plaintext
NSales/
 ├── NSales.API/         # Endpoints and controllers
 ├── NSales.Application/ # Use cases and business rules
 ├── NSales.Domain/      # Domain entities and interfaces
 ├── NSales.Infrastructure/ # Persistence and external integrations
 ├── NSales.Tests/        # Automated tests
 └── README.md
```

------------------------------------------------------------------------

## ⚡ How to Run

1.  **Clone the Repository**

    ``` bash
    git clone https://github.com/yourorg/nsales.git
    cd nsales
    ```

2.  **Configure Environment Variables**\
    Create an `appsettings.json` file with database and external service
    configurations.

3.  **Run with Docker**

    ``` bash
    docker-compose up --build
    ```

4.  **Access the API**

        http://localhost:5000/swagger

------------------------------------------------------------------------

## 🧪 Tests

Run unit tests:

``` bash
dotnet test
```

------------------------------------------------------------------------

## 📄 License

This project is licensed under the MIT License - see the
[LICENSE](LICENSE) file for details.
