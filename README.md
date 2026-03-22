# Broadcast Server

A simple CLI-based broadcast server built with WebSockets that enables real-time communication between multiple connected clients. The server listens for incoming connections and broadcasts messages sent by any client to all others, simulating the core behavior of chat applications and live systems.

This project demonstrates how to handle multiple client connections, message broadcasting, and graceful disconnections in a real-time environment.

## Getting Started
1. Clone the repository:
   ```bash
   git clone https://github.com/angellisandroerazo/broadcast-server.git
   ```
2. Navigate to the project directory:
   ```bash
   cd broadcast-server
   ```
3. Open the project in Visual Studio 2022

4. Run the application

### Run the application with the Terminal
1. This project contains two console applications:

server → to start the broadcast server
client → to connect as a client

Open a terminal and navigate to the corresponding folder depending on what you want to run.

2. Run the server
```bash
cd BroadcastServer
dotnet run 
```
3. Run the client
```bash
cd BroadcastClient
dotnet run 
```

You can open multiple terminals and run the client multiple times to simulate multiple connected users.

## Usage
- Start the server first
- Then run one or more clients
- Any message sent from a client will be broadcast to all connected clients in real time

## Problem Statement

This project addresses a task management problem inspired by the challenges outlined in the [Broadcast Server](https://roadmap.sh/projects/broadcast-server).
