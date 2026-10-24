# Ctrader API

A cTrader robot and a .NET client for the account on that chart. The robot stays on the chart. Your program asks it for balances, history, and positions.

---

## Overview

- Account name, balance, equity, and server time.
- Open positions and account history.
- Close one position or close all.

---

## Tech Stack

| Component | Technology |
| --- | --- |
| **Language** | C# |
| **Trading** | cTrader / cAlgo |
| **Client** | .NET Framework, WCF |

---

## Project Structure

```
CtraderAPI/
├── CtApiService.sln
├── CTApiService/       # Shared service
├── CtApi/              # .NET client
└── CtApi cBot/         # Robot for cTrader
```

---

## Prerequisites

- Windows
- Visual Studio with the .NET desktop workload
- cTrader

---

## Setup

Open `CtApiService.sln`. Build `CTApiService` in Release, then build `CtApi`.

In cTrader, compile `CtApi cBot/CtApi cBot.cs` and add it to a chart.

---

## Run

Start the robot. It prints the port it is using. The default is `2529`.

```csharp
var client = new CtClient();
client.Connect(port);
double balance = client.AccountBalance();
var open = client.AccountPositions();
client.Disconnect();
```

Call `Connect` before the other methods.
