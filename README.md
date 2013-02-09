Nova
====

A Sample application to demonstrate WCF Data Services over ODATA using Entity Framework 5.

# New Technologies Used 
I selected the most recent technologies for a Visual Studio 2010 requirement someone gave me. 
I wanted to showcase how you use these technologies in order to satisfy the functional requirements given to me with a modern approach. At the same time I also tried to incorporate various aspects of my skill set in the development process. 

## NuGet 
### Download: 
http://nuget.codeplex.com 
### Motivation: 
NuGet makes it easy to download extensions and the latest Microsoft packages. You can do so by simply right clicking on a project and selecting 'Manage NuGet Packages.

## SQL Server Data Tools 2012 (Dec 2012 Release) 
### Download: 
http://msdn.microsoft.com/en-us/data/hh297027 
### Information: 
http://msdn.microsoft.com/en-us/data/tools.aspx 
### Motivation: 
SQL Server Data Tools takes the pain out of database development by adding a new ﬂexible project type called a SQL Server Database Project. I have been using this technology for more than 2 years now and used to be called "Data-Tier Application Projects" and has simpliﬁed roll-outs and the development process dramatically.
By simply right clicking on the project you can deploy to a new database or update an existing database. You also can create a DacPac module that makes it easy to upgrade production environments and SQL Azure cloud environments without maintaining update scripts. It also has the ability to generate a "Diff" script for traditional enterprise on site environments.  

## Entity Framework 5.0 
### Download: 
Please Use NuGet. See http://msdn.microsoft.com/en-us/data/ef.aspx 

### Motivation: 
Entity framework takes the pain out of database driven development and has brilliant extensions like OData WCF Data Services that makes this framework one of the best frameworks to use for LOB applications. I initially used a Model First approach to build my entity model and generate the initial database before importing it into a SSDT project where I can then update the model to reﬂect changes made to the database.

## WPF
Plain old simple WPF. The point was not to make it to fancy. Illustrates how entity databindings work and also how LINQ over OData works. 
The UI was written to be Multi-Threaded and works quite well. It also illustrates the use of Timers for purposes of synching user selections.
I also experemented with a slightly different approach from MVC or MVVM. I call it MVTD, Model View Target Delegate :)
