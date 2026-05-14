# Recipe Table App - Backend
## Description
This is the backend repository of the recipe table app. Further information can be found at the [recipe-app-frontend repo](https://github.com/akali013/recipe-app-frontend).

## Technologies Used
- Figma for creating the [Entity-Relationship Diagram](https://www.figma.com/board/heKG9RTu6pV3hGdWF0mxGS/Recipe-App-ERD?node-id=0-1&p=f)
- .NET v9
- SQL Server Management Studio 2022
- Postman for API Testing
- Docker Desktop/Kubernetes
- Python v3.13.1 for data preprocessing

## Challenges Faced
### Kubernetes setup 
- Saving recipe data in the SQL Server and recipe image data in .NET required learning how to use local and hostPath persistent volumes (PVs) and persistent volume claims (PVCs).
- Coordinating the frontend service with the backend service from the external browser required using two NodePort services on ports 30001 (frontend) and 30002 (backend).
- Connecting the .NET service with the SQL Server stateful set required configuring the .NET connection string to include the stateful set's name and allowing enough time for the SQL Server to start via a readiness probe and retry checks within .NET's program.cs.
### Handling Image Files
- Recipe image data is saved throughout the backend by storing the image data in .NET's /wwwroot directory and the paths to specific images in this directory in the SQL Server database.

## How to Run
> [!IMPORTANT]
> The frontend must be running as well for the app to work. It can be found at the [recipe-app-frontend repo](https://github.com/akali013/recipe-app-frontend).
> [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/) and [Kubernetes with kubectl](https://kubernetes.io/releases/download/) must also be installed.

1. If you haven't already, start a Kubeadm cluster in the Kubernetes section of Docker Desktop.
2. In any IDE, clone this repo using `git clone https://github.com/akali013/recipe-app-backend.git`.
3. Navigate to the recipe-app-backend directory with `cd recipe-app-backend`.
4. In a terminal, run `kubectl apply -f recipe-app-backend-kubernetes.yaml` to create the backend deployment, stateful set, PVs, and PVCs. The output should be `secret/db-password-secret created
persistentvolume/recipe-data-volume created
persistentvolumeclaim/recipe-data-pvc created
storageclass.storage.k8s.io/local-storage created
persistentvolume/recipe-images-volume created
persistentvolumeclaim/recipe-images-pvc created
deployment.apps/recipe-app-backend-server created
statefulset.apps/recipe-app-backend-db created
service/recipe-app-backend-server created
Warning: spec.SessionAffinity is ignored for headless services
service/recipe-app-backend-db created`.
5. The backend is now available for requests at localhost:30002 from the frontend.
> [!NOTE]
> It will take some time for the SQL Server container to run. The backend is fully operational once the recipe-app-backend container has `Now listening on: http://0.0.0.0:5132⁠` in its logs.

## Credits
- [The MealDB API](https://www.themealdb.com/api.php)
- [.NET JWT Authentication](https://jasonwatmore.com/post/2022/02/26/net-6-boilerplate-api-tutorial-with-email-sign-up-verification-authentication-forgot-password) by Jason Watmore
- [.NET Docker/Kubernetes setup](https://docs.docker.com/guides/dotnet/)
