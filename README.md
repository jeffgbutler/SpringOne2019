# PCF 101 with .Net Core, Steeltoe, and Redis
This repository contains code for a basic .Net microservice that will deploy to Pivotal Cloud Foundry. The microservice is a loan payment calculator. There is an endpoint where a user can supply:

- The loan amount
- The yearly interest rate
- The number of years

The microservice will calculate the loan payment, and will return a result. The result contains all input fields, and the calculated payment.

The service also contains a hit counter and will return the total hit count for the application. When running locally, the hit counter will be memory based and will reset everytime the application is started. When running on PCF, the hit counter will use a Redis cache to provide a consistent hit count across deployments and across scaled application instances.

When the service is scaled on PCF, the result will also contain an indication of which application instance processed the request.

The application includes a [Vue.js](https://vuejs.org/) based single page web application (SPA) that can be used to randomly generate traffic for the microservice and demonstrate different features in PCF.

The service also includes a Swagger UI that can be used to exercise the endpoints. In this service, we use the Swashbuckle swagger implementation: https://github.com/domaindrivendev/Swashbuckle.AspNetCore

The service uses [Steeltoe](https://steeltoe.io/) and demonstrates the following parts of Steeltoe:

1. Management Endpoints: https://steeltoe.io/docs/steeltoe-management/#1-0-management-endpoints
1. Service Connectors - specifically the Redis connector: https://steeltoe.io/docs/steeltoe-connectors/#5-0-redis
1. Logging: https://steeltoe.io/docs/steeltoe-logging/
1. Configuration - specifically the Cloud Foundry provider: https://steeltoe.io/docs/steeltoe-configuration/#1-0-cloud-foundry-provider

# Running the Demo

## Running Locally

1. Install the .Net core SDK from this URL: https://dotnet.microsoft.com/download
1. Verify the install by opening a terminal or command window and typing `dotnet --version`. You should see a version string to match the version you installed
1. Clone the repo from Github
1. From the main directory of the cloned repo, enter `dotnet run`, then navigate to https://localhost:5001

You can also easily run the app from Visual Studio Code:

1. Install Visual Studio Code from this URL: https://visualstudio.microsoft.com/
1. Install the C# extension for VS Code
1. Open the application root directory in VS Code
1. Allow VS code to create the necessary assets for running the application
1. Press F5

## Running on PCF

1. Install the Cloud Foundry CLI from this URL: https://docs.cloudfoundry.org/cf-cli/install-go-cli.html
1. Verify the install by opening a terminal or command window and typing `cf --version`. You should see a version string to match the version you installed
1. If you are using a private installation of PCF, then obtain credentials and API enpoint information from your PCF platform team. If you are using Pivotal Web Services (the public PCF instance hosted by Pivotal), then go to [https://run.pivotal.io/](https://run.pivotal.io/) and register for a free account.
1. Log in to the application manager for your PCF instance (https://run.pivotal.io if using Pivotal Web Services). Create a Redis cache service in your PCF environment. On Pivotal Web Services, add a Redis Cloud instance using the 30MB (free) plan. Name the service "PaymentCalculatorRedis" (If you use a different name, you will need to update the manifest file [manifest.yml](manifest.yml))
1. Login with the CLI...
    1. Open a terminal or command window and login to PCF with the command `cf login -a api.run.pivotal.io` (or whatever API endpoint you are using if not Pivotal Web Services)
    1. Enter the email you registered and the password you set
1. Execute `cf push` from the application root directory. Make note of the route created for the application (for example, it might be something like "paymentservice-10-persistent-oryx.cfapps.io")
1. Navigate to the application in a browser (for example: https://paymentservice-10-persistent-oryx.cfapps.io)

# Basic Demo Script

1. Explain the basic function of the application - a payment calculator
1. Explain that the same application code will run locally and on PCF without modification. When deployed to PCF, the application will automatically attach to a Redis cache
1. Run the application locally -
    - Start the application from VS Code
    - Browser should open to the local root: https://localhost:5001
    - Press the "Start" button - the client application will start generating traffic to the microservice
    - Show the Swagger UI: https://localhost:5001/SwaggerUI
    - Show the actuators: https://localhost:5001/actuator
    - Make the point that the hit counter is memory based and will not persist. You can demo that by stopping the app, then restartring it
1. Run the application on PCF -
    - Navigate to the application root
    - Start traffic flowing to the application - note that PCF Application Instance is always "0"
    - Scale the app up to two instances (either with `cf scale` or through the app manager UI)
    - Show traffic being load balanced acrss the two instances
    - Press the "Crash It!" button - notice that traffic is only flowing to a single instance for a while, but eventually there will be two instances again (PCF notices the crash and restarts an instance)
    - Scale the application down to a single instance
    - Show integration of the application into the app manager UI
        - Steeltoe Icon
        - Detailed Health Indicator
        - Settings -> Steeltoe Info
        - Settings -> Mappings
        - Dynamic Log Configuration (Turn the PaymentController log to Debug, then off)
        - SpringBoot Trace
    - In app manager, set the logging level for "PaymentController" to "DEBUG". Tail the logs to show the debug message from the controller. Set the logging leve to "OFF" - show that the message is no longer being generated

# For Developers...
Show how the application is coded. It is a "normal" ASP.NET Core application, with the following additions:

- [PaymentService.csproj](PaymentService.csproj) has package references for Steeltoe and Swashbuckle
- [appsettings.json](appsettings.json) has an app name configured for the info endpoint
- [appsettings.Production.json](appsettings.Production.json) changes the management endpoints URL to a value appropriate for PCF (default is /actuator like SpringBoot)
- [RedisHitCountService](Services/RedisHitCountService.cs) has an `IConnectionMultiplexer` injected to give access to Redis. This comes from PCF and Steeltoe (see below)
- [Program.cs](Program.cs) has the following Steeltoe additions:
    - `UseCloudFoundryHosting`
    - `AddCloudFoundry`
    - `AddDynamicConsole` in the logging configuration - this is the hook into the logging UI in PCF app manager
- [Startup.cs](Startup.cs) has the following Steeltoe additions:
    - `AddRedisConnectionMiltiplexer` - automatically binds to redis service instance bound to the app in PCF
    - `AddCloudFoundryActuators` and `UseCloudFoundryActuators` - turns on the management endpoints (Spring actuator)
- [Startup.cs](Startup.cs) configures the app for a memory based hit counter in the development environment (default local environment), otherwise uses the Redis implementation. PCF looks like "Production" by default
- [PaymentController](Controllers/PaymentController.cs) has `IOptions<CloudFoundryApplicationOptions>` injected - this gives access to the application instance index. Steeltoe defaults this to "-1" when running locally
