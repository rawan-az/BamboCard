# Currency Exchange API

## Overview
This is a C# ASP.NET Core microservice for currency conversion and historical exchange rate retrieval.  
It integrates with external currency providers like **Frankfurter API** and uses **caching, retry policies, and a circuit breaker** for reliability.

## Features
- Convert amounts between different currencies.
- Retrieve historical exchange rates with pagination.
- Implement caching to reduce API calls.
- Use retry policies with exponential backoff.
- Introduce a circuit breaker to handle API outages.
- Dynamically select a currency provider using the **Factory Pattern**.

---

## 🔧 Setup Instructions

### **Prerequisites**
- .NET 7.0 or later
- Docker (optional for running in a container)
- Redis (for caching)
- RabbitMQ (if event-driven architecture is required)

### **1. Clone the Repository**
```sh
git clone https://github.com/your-username/your-repo-name.git
cd your-repo-name
