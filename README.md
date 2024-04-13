# Adoptly web application

## Overview
Adoptly aims to connect people with pets from animal shelters. The application has three user types: adopters, shelters and admins. Features included in the application are as follows. 

- User registration with email verification.
- User log in and log out with session management.
- Automatic removal of unverified accounts after 28 days.
- Admin analytics dashboard with statistics and a CSV download option.
- Profile management for all users and pets. 
- Social media integration.
- Pet matchmaking quiz for adopters.
- Pet search tool.
- Adoption request form and management system.

The application is written in C# using ASP.NET Core as the web framework and EF Core as the ORM. Services used include Microsoft Azure (App Service, SQL Database, AI Search and Blob Storage), Gmail SMTP Server, Google Charts and Facebook Social Plugin.

Adoptly was developed in a team. I took the role of Technical Lead, which involved choosing the technologies and overseeing all development work. I also worked on the backend and was solely responsible for implementing the matchmaking algorithm and the pet search feature. 

Note that the application is no longer live on the Azure platform. 

## Architectural diagram

<img src="https://github.com/CarelleRichards/adoptly/assets/137973963/7516329c-59a1-4bc1-be91-b9ecb8e7a071"><br>

## Database entity-relationship diagram

Adoptly implements a relational database using Azure SQL Database.

<img src="https://github.com/CarelleRichards/adoptly/assets/137973963/1ce5aef0-d85f-4082-9a6e-efa62e762d08"><br>

## Application preview screenshots

### Register
<img src="https://github.com/CarelleRichards/adoptly/assets/137973963/faa93332-0446-4f4c-b4c9-d1584af2f1c6" width=800px>

### Log in
<img src="https://github.com/CarelleRichards/adoptly/assets/137973963/c5f88dbf-0086-49c0-b750-ebbdcad35ec1" width=800px>

### Adopter dashboard
<img src="https://github.com/CarelleRichards/adoptly/assets/137973963/ab3f546c-fc6c-40d0-8d00-8ebb8e2eec7e" width=800px>

### Pet search
<img src="https://github.com/CarelleRichards/adoptly/assets/137973963/aa77020b-ddf4-461d-8577-9e72738e13ad" width=800px>

## Pet matchmaking quiz
<img src="https://github.com/CarelleRichards/adoptly/assets/137973963/338f1741-589c-41ee-98c9-042916c78332" width=800px>

### Pet matchmaking quiz results
<img src="https://github.com/CarelleRichards/adoptly/assets/137973963/7c730ad0-bd84-4719-b7e9-eb5b96c99241" width=800px>

### Pet profile
<img src="https://github.com/CarelleRichards/adoptly/assets/137973963/4e1d5a32-dca3-4258-a443-b3b2ca906b19" width=800px>

### Pet application form
<img src="https://github.com/CarelleRichards/adoptly/assets/137973963/9c97d0b6-4ebc-4e56-9c3f-b9c00aa3f201" width=800px>

### Admin analytics dashboard
<img src="https://github.com/CarelleRichards/adoptly/assets/137973963/6870c8cc-1548-4556-92c1-53e05ed2c2fc" width=800px>

## References

[View references cited within the code ➡️](https://github.com/CarelleRichards/adoptly/wiki/References)
