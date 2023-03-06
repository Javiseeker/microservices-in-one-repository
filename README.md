# Monorepo Microservices
Sample repository that will hold deployable microservices as individual .NET 7 Projects managed by 1 solution.

AMR stands for All Microservices in one repository. MonoRepository and Monosolution approach.

## Interesting Links:

https://dev.to/willvelida/dapr-service-invocation-with-azure-container-apps-41p8
https://learn.microsoft.com/en-us/visualstudio/containers/tutorial-multicontainer?view=vs-2022

## New microservice setup
1. Change to the Folder View in VS 2022.
2. Add a new folder with the microservice name.
3. Add src and tests folders within the new folder.
4. Change to the solution workspace by double click the solution file.
5. Right click repository's solution and add a new Web API project under the new folder/src. Be careful to match the same folders that were created in the Folder View.
6. Use the following the naming conventions {SolutionNamespace}.{MicroserviceName}.{DesiredFolders}. See current microservices for an example.
6.1 Add other projects under src as required. Example, Business and Infrastructure class projects.
7. Modify the launchSettings.json to add the Docker configuration:

```
"Docker": {
      "commandName": "Docker",
      "launchUrl": "{Scheme}://{ServiceHost}:{ServicePort}/swagger",
      "publishAllPorts": true,
      "useSSL": true
}
```
7.1 While you are at it, set up all the other ports so that they won't clash with other microservices. Always use the same pattern for cleanliness. Example: 5201,5202,5203.
7.2 Be careful not to modify the IISExpress SSL port.
7.3 Run the application in all possible modes to double check everything is working okay. If triggered to sign a SSL certificate, click yes.
8. Right click the newly added api and follow the steps: Add > Container Orchestrator Support. Choose Docker Compose and Linux Target OS.

9. A new docker-compose project will be linked to the solution as well as auto-generated Dockerfile for the new microservice.

10. Change to the Folder View and open the docker-compose.override.yml. Update the ports field to the port and ssl port where your new microservice will be hosted.
Be careful to follow the previous port conventions you set up.


## Containers SSL setup
To properly set up the SSL configuration for the containers follow these steps:

1. Right click you newly added web API project and click Manage User Secrets.
2. Replace the contents with the following: 
```
{
  "Kestrel": {
    "Certificates": {
      "Default": {
        "Path": "/root/.aspnet/https/<<YourMicroserviceName>>.pfx",
        "Password": "<<ThisKestrelPasswordShouldAlreadyBeHereOnceYouRanTheApp>>"
      }
    }
  }
}
```