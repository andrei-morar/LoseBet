LoseBet - Full-Stack Casino Platform
LoseBet is a modern, high-performance online gambling platform built with WPF (C#) for the desktop client and ASP.NET Core for the backend API. The platform offers a seamless, real-time gaming experience with interactive features.

🚀 Key Features
Aviator Pro: Real-time exponential flight simulation with Auto Cashout functionality.

Triviador Arena: Strategy-based quiz game on an interactive vector-based map of Romania (42 counties).

Alba-Neagra (Shell Game): Classic cup-and-ball game with fluid animations.

Sports Betting: Real-time odds calculation and dynamic bet slip management.

Admin Panel: Full administrative control for user management, slot configuration, and live trivia question management.

🛠️ Technical Stack
Backend
ASP.NET Core Web API

Entity Framework Core with SQL Server

Real-time data handling for game states and balances

Frontend (Desktop Client)
WPF (Windows Presentation Foundation)

XAML/C# MVVM-lite architecture

Canvas & Path Geometry for interactive vector maps

DispatcherTimer for smooth real-time animations

🎮 How it works
The Triviador Engine
Unlike traditional trivia games, Triviador features an interactive vector map. We utilize SVG paths parsed directly into C# Geometry to render all 42 counties of Romania. The game engine integrates a bot-based turn system, ensuring a competitive experience even in single-player mode.

Aviator Mechanics
The flight engine uses a server-side crash point generator to ensure fair play, combined with a local high-frequency dispatcher timer (30ms) for ultra-smooth flight rendering on the client side.

🛡️ Admin & Security
Role-Based Access Control: Secure Admin Panel with user banning, balance adjustment, and role elevation capabilities.

Dynamic Data: Trivia questions can be added, managed, and deleted via the Admin Panel, syncing directly with the database.

📸 Screenshots
Coming soon: Replace with paths to your screenshots (e.g., screenshots/aviator.png)

⚙️ Setup
Clone the repository:

Bash
git clone https://github.com/your-username/LoseBet.git
Database Migration:
Run the migrations in the API project to initialize the database:

Bash
dotnet ef database update
Run API: Start the ASP.NET Core project.

Launch Desktop Client: Build and run the WPF project.

💡 Built by
[Your Name/Username] - Lead Developer
