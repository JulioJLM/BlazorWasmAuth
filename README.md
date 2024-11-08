# Standalone Blazor WebAssembly with ASP.NET Core Identity


## Steps to run

1. The default and fallback URLs for the two apps are:

   * `Backend` app (`BackendUrl`): `https://localhost:7211` (fallback: `https://localhost:5001`)
   * `BlazorWasmAuth` app (`FrontendUrl`): `https://localhost:7171` (fallback: `https://localhost:5002`)
   
1. If you plan to run the apps using the .NET CLI with `dotnet run`, note that first launch profile in the launch settings file is used to run an app, which is the insecure `http` profile (HTTP protocol). To run the apps securely (HTTPS protocol), take ***either*** of the following approaches:

   * Pass the launch profile option to the command when running the apps: `dotnet run -lp https`.
   * In the launch settings files (`Properties/launchSettings.json`) ***of both projects***, rotate the `https` profiles to the top, placing them above the `http` profiles.
  
   If you use Visual Studio to run the apps, Visual Studio automatically uses the `https` launch profile. No action is required to run the apps securely when using Visual Studio.

1. Run the `Backend` and `BlazorWasmAuth` apps.

1. Navigate to the `BlazorWasmAuth` app at the `FrontendUrl`.

1. Register a new user using the **Register** link in the upper-right corner of the app's UI or use one of the preregistered test users:

   * `leela@contoso.com` (Password: `Passw0rd!`). Leela has `Administrator`, `Manager`, and `User` roles and can access the private manager page but not the private editor page of the app. She can process data with both forms on the data processing page.
   * `harry@contoso.com` (Password: `Passw0rd!`). Harry only has the `User` role and can't access the manager and editor pages. He can only process data with the first form on the data processing page.

1. Log in with the user.

1. Navigate to the Todo Items page (`Components/Pages/TodoItems.razor` at `/todo-items`) that only authenticated users can reach. A link to the page appears in the navigation sidebar after the user is authenticated.

1. Log out of the app.
