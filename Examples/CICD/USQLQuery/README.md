This sample provides a sample solution structure for U-SQL application development. The solution contains U-SQL query scripts, U-SQL database definition, C# user defined operator, test cases for U-SQL scripts and C# user defined operator, and deployment PowerShell script for CI/CD scenario.

Usually, you can organize the your solution in the way show as follows:

- U-SQL Project --- manage U-SQL query script
- U-SQL Database Project --- manage U-SQL database definitions. One project for one database is preferred.
- Class Library (For U-SQL Application) --- manage C# user defined operators
- U-SQL Script Unit Test Project --- manage test cases for U-SQL scripts
- U-SQL C# UDO Unit Test Project --- manage test cases for C# user defined operators

# Project reference

- A U-SQL Project can reference a U-SQL Database Project, which means queries in this U-SQL project need this database environment for build and run.
- A U-SQL Database Project can reference a Class Library (For U-SQL Application) project. Assemblies in database can be created by referenced C# source code.
- A U-SQL Script Unit Test Project can reference a U-SQL Project to set up and run test cases for scripts in the U-SQL project.
- A U-SQL C# UDO Unit Test Project can reference a Class Library (For U-SQL Application) project. Test cases for C# user defined operators can be created and run then.

# MSBuild command 

- [Build U-SQL project](https://docs.microsoft.com/azure/data-lake-analytics/data-lake-analytics-cicd-overview#build-u-sql-project)
- [Build U-SQL database project](https://docs.microsoft.com/azure/data-lake-analytics/data-lake-analytics-cicd-overview#build-u-sql-database-project)
- Build other project: Class Library (For U-SQL Application), U-SQL Script Unit Test Project and U-SQL C# UDO Unit Test Project all inherit from C# project, you can just use the MSBuild command without passing parameter for build.

# Set up CI/CD pipeline

- [How to set up CI/CD pipeline for Azure Data Lake Analytics](https://docs.microsoft.com/azure/data-lake-analytics/data-lake-analytics-cicd-overview#build-u-sql-database-project)

# Questions, comments or feedback

Please mail adldevtool@microsoft.com with your questions, comments or feedback.

