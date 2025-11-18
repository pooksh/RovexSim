# RovexSim
RovexSim is a marketing tool which aims to simulate the use of Rovi (robot-stretchers) within a hospital environment.

# Backend/DB Setup in Docker
Make sure docker desktop is installed and PATH variable set up for docker compose commands

Open a cmd/PS window and cd to ..\Backend\src\docker-compose\

Run the following command to build the backend API and SQL containers: 

`docker compose up`

Open a separate cmd/PS and cd to ..\Backend\src\docker-compose\init-db\

Run the following commands to initialize an empty database:

`winget install sqlcmd`

`sqlcmd -S localhost,1433 -U sa -P YourStrong!Pass123 -i 01-init.sql`

Now, you should be able to register users and validate logins using the backend. By default, it should be available at http://localhost:5000/swagger/index.html

# Backend Setup in Visual Studio
Use sln, vsproj, and dcproj files posted in the discord if you want to just open the projects in Visual Studio.  