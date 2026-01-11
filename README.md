# Backlog Basement - .NET REST API Backend

A clean, extensible .NET REST API backend for Backlog Basement, a website that allows users to track owned (mostly retro) video games, manage their personal collection, and log time spent playing each game.

## 🎮 Features

### User Features
- Authenticate using Google OAuth
- Maintain a personal game collection (portfolio)
- Add games from a shared global game catalog (sourced from IGDB)
- Remove games from their collection
- Log time spent playing each owned game

### System Features
- Fetch and cache game metadata from the IGDB API
- Persist user data and collections using SQLite
- Expose a RESTful API for frontend applications
- Clean, layered architecture
- Dependency injection throughout

## 🏗️ Architecture

The application follows a clean, layered architecture:

```
Controllers → Services → Repositories/DbContext → SQLite Database
                     ↓
                External APIs (IGDB)
```

- **Controllers**: Thin layer for HTTP handling and input validation
- **Services**: Business logic and authorization enforcement
- **Data**: Entity Framework Core with SQLite database
- **External Integration**: IGDB API service for game data

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core Web API (.NET 8)
- **ORM**: Entity Framework Core
- **Database**: SQLite (initially; schema is portable to PostgreSQL/SQL Server)
- **Authentication**: Google OAuth (via ASP.NET Core Authentication)
- **External API**: IGDB API (Twitch OAuth flow)
- **Architecture**: Layered architecture with Dependency Injection

## 🔧 Setup Instructions

### Prerequisites

- .NET 8 SDK or later
- SQLite (or SQLite tools)

### 1. Configure Authentication

#### Google OAuth Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google+ API
4. Go to Credentials → Create Credentials → OAuth client ID
5. Configure the OAuth consent screen if prompted
6. Add authorized redirect URI: `https://localhost:5001/auth/callback/google`
7. Copy the Client ID and Client Secret

#### Configuration

Update `appsettings.json` with your Google OAuth credentials:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  }
}
```

### 2. Configure IGDB Integration

1. Create a Twitch account if you don't have one
2. Go to [Twitch Developers](https://dev.twitch.tv/)
3. Create a new application
4. Get your Client ID and Client Secret
5. Add `https://localhost:5001` as an OAuth redirect URI (for future use)

Update `appsettings.json` with your IGDB credentials:

```json
{
  "Igdb": {
    "ClientId": "YOUR_IGDB_CLIENT_ID",
    "ClientSecret": "YOUR_IGDB_CLIENT_SECRET"
  }
}
```

## 🚀 Quick Start

```bash
# Build the project
dotnet build

# Run the project
dotnet run
```

The API will be available at `https://localhost:5001`

Swagger UI will be available at `https://localhost:5001/swagger` in development mode.

