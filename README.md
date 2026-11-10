# AmazonCreators.Net

[![CI Build & Test](https://github.com/molidestroyer/AmazonCreators.Net/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/molidestroyer/AmazonCreators.Net/actions)
[![NuGet Version](https://img.shields.io/nuget/v/AmazonCreators.Net.svg)](https://www.nuget.org/packages/AmazonCreators.Net)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 8.0 | 9.0 | 10.0](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![MCP Ready](https://img.shields.io/badge/MCP-Model%20Context%20Protocol-green.svg)](https://modelcontextprotocol.io/)

A modern, high-performance, resilient .NET client and **Model Context Protocol (MCP)** server for the **Amazon Creators API** (formerly PA-API 5.0).

---

## 💡 Overview

In February 2026, Amazon retired PA-API 5.0 and transitioned Creators/Associates to the new **Amazon Creators API** (`/catalog/v1/`).

**AmazonCreators.Net** is a modern, high-performance, and resilient .NET client and Model Context Protocol (MCP) server that provides full, unrestricted access to the Amazon Creators API under the permissive **MIT License**.

### Key Features & Architectural Highlights:
- 🔓 **Full Unrestricted Access**: Complete support for `OffersV2` (all merchant listings, list price, savings basis, Buy Box winner, condition), `ItemInfo`, `Images`, `CustomerReviews`, and `SearchItems`.
- 🔑 **Dual Authentication Architecture**:
  - **v3.x (Login with Amazon - LwA)**: Native support for v3.1 (US), v3.2 (EU / Spain), and v3.3 (Japan) using standard Bearer authentication, JSON credentials body, and `creatorsapi::default` scope.
  - **v2.x (Amazon Cognito)**: Backward compatible with legacy Cognito endpoints, form-urlencoded token exchange, and versioned Authorization headers (`Bearer {token}, Version {version}`).
- 🛡️ **Preventive Rate Limiting**: Built-in client-side **Token Bucket Rate Limiter** powered by .NET's `System.Threading.RateLimiting`. Requests are smoothed proactively in-process rather than blindly hitting Amazon's aggressive 429 limits.
- ⚡ **Resilience by Design (Polly)**: Automatic exponential backoff with jitter on HTTP 429 (Too Many Requests) and transient 5xx errors, combined with an intelligent circuit breaker.
- 🤖 **Model Context Protocol (MCP) Server**: Includes `AmazonCreators.Mcp`, a production-ready stdio MCP server for AI coding assistants (Claude Code, Cursor, Antigravity, Claude Desktop) to fetch real-time Amazon pricing and catalog data.
- 🌐 **Multi-Marketplace Routing**: Seamlessly route queries to different Amazon marketplaces (e.g. `www.amazon.es`, `www.amazon.com`, `www.amazon.de`) and partner tags on a per-request basis.
- 🚀 **Zero Dependency Bloat**: Uses `System.Text.Json` (no Newtonsoft dependency), trim-friendly, and compatible with .NET 8, .NET 9, and .NET 10.

---

## 📦 Installation

Install the library via NuGet:

```bash
dotnet add package AmazonCreators.Net
```

Or via Package Manager Console:

```powershell
Install-Package AmazonCreators.Net
```

---

## 🚀 Quick Start

### 1. Minimal Direct Instantiation

```csharp
using AmazonCreators.Net.Configuration;
using AmazonCreators.Net.Services;

var options = new AmazonCreatorsOptions
{
    ClientId = "amzn1.oa2-cs.v1.YOUR_CLIENT_ID",
    ClientSecret = "YOUR_CLIENT_SECRET",
    PartnerTag = "yourtag-21",
    Marketplace = AmazonMarketplace.Spain,
    CredentialVersion = "3.2" // Login with Amazon (v3.2 for EU/Spain)
};

using var httpClient = new HttpClient();
var client = new AmazonCreatorsClient(httpClient, options);

// Fetch current display price
var price = await client.GetProductPriceAsync("B08N5WRWNW");
Console.WriteLine($"Price: {price}");
```

### 2. Enterprise Dependency Injection (Recommended)

In your `appsettings.json`:

```json
{
  "AmazonCreators": {
    "ClientId": "amzn1.oa2-cs.v1.YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "PartnerTag": "yourstore-21",
    "Marketplace": "www.amazon.es",
    "CredentialVersion": "3.2",
    "RequestTimeoutSeconds": 30,
    "MaxRetryAttempts": 3,
    "EnableResponseCaching": true,
    "ResponseCacheDurationSeconds": 300
  }
}
```

In `Program.cs` / `Startup.cs`, choose the method that best fits your project:

**Option A: Quick Direct Registration (Zero Boilerplate)**
```csharp
using AmazonCreators.Net.Configuration;
using AmazonCreators.Net.Extensions;

builder.Services.AddAmazonCreatorsClient(
    clientId: "amzn1.oa2-cs.v1.YOUR_CLIENT_ID",
    clientSecret: "YOUR_CLIENT_SECRET",
    partnerTag: "yourstore-21",
    marketplace: AmazonMarketplace.Spain, // optional, default: Spain
    credentialVersion: "3.2");            // optional, default: 3.2
```

**Option B: From Configuration (`appsettings.json`)**
```csharp
// Binds from section "AmazonCreators" by default
builder.Services.AddAmazonCreatorsClient(builder.Configuration);
```

**Option C: Fluent Action Delegate**
```csharp
builder.Services.AddAmazonCreatorsClient(options =>
{
    options.ClientId = "amzn1.oa2-cs.v1.YOUR_CLIENT_ID";
    options.ClientSecret = "YOUR_CLIENT_SECRET";
    options.PartnerTag = "yourstore-21";
    options.Marketplace = AmazonMarketplace.Spain;
    options.CredentialVersion = "3.2";
});
```

Then inject `IAmazonCreatorsClient` anywhere:

```csharp
public class DealCheckerService(IAmazonCreatorsClient amazonClient)
{
    public async Task CheckPriceAsync(string asin)
    {
        var offers = await amazonClient.GetProductOffersV2Async(asin);
        var bestListing = offers?.Listings?.FirstOrDefault(l => l.IsBuyBoxWinner);
        
        if (bestListing?.Price?.Money != null)
        {
            Console.WriteLine($"Buy Box Winner: {bestListing.Price.Money.DisplayAmount}");
        }
    }
}
```

---

## 🛠️ Usage Examples

### Fetch Complete Product Metadata & Images

```csharp
var response = await client.GetProductOffersAsync(["B08N5WRWNW"]);
var item = response?.FirstItem;

if (item != null)
{
    Console.WriteLine($"Title: {item.GetTitle()}");
    Console.WriteLine($"Brand: {item.GetBrand()}");
    Console.WriteLine($"Best Price: {item.GetBestPrice()?.DisplayAmount}");
    Console.WriteLine($"Reference Price: {item.GetRegularPrice()}");
    Console.WriteLine($"Savings: -{item.GetMaxSavingsPercentage()}%");
    Console.WriteLine($"Image: {item.GetPrimaryImageUrl()}");
}
```

### Multi-Marketplace Routing

Query different countries with their corresponding partner tags on the fly:

```csharp
var targetGermany = new AmazonMarketplaceTarget(
    Marketplace: AmazonMarketplace.Germany, 
    PartnerTag: "yourstore-de-21");

var responseDe = await client.GetProductOffersAsync(["B08N5WRWNW"], targetGermany);
```

### Search by Keyword or Barcode / EAN

```csharp
// Search by 13-digit EAN barcode
var searchResponse = await client.SearchItemsAsync("0711719541028");

foreach (var result in searchResponse?.Items ?? [])
{
    Console.WriteLine($"[{result.Asin}] {result.GetTitle()} - {result.DetailPageUrl}");
}
```

---

## 🤖 Model Context Protocol (MCP) Server

`AmazonCreators.Net` includes a built-in MCP server (`AmazonCreators.Mcp`) designed for integration with AI assistants.

### Available MCP Tools:
- `amazon_get_price`: Fast price, stock, saving basis, and Buy Box lookup for an ASIN.
- `amazon_get_product`: Complete structured product JSON (offers, seller info, high-res images, EAN).
- `amazon_search`: Search Amazon's catalog by text or EAN.
- `amazon_health_check`: Test API credentials and verify active token status.

### Configuration for Claude Desktop / Claude Code / Antigravity

Add the server to your `claude_desktop_config.json` or `.mcp.json`:

```json
{
  "mcpServers": {
    "amazon-creators": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:/repos/AmazonCreators.Net/src/AmazonCreators.Mcp/AmazonCreators.Mcp.csproj",
        "-c",
        "Release"
      ],
      "env": {
        "AMAZON_CREATORS_CLIENT_ID": "amzn1.oa2-cs.v1.YOUR_CLIENT_ID",
        "AMAZON_CREATORS_CLIENT_SECRET": "YOUR_CLIENT_SECRET",
        "AMAZON_CREATORS_PARTNER_TAG": "yourstore-21",
        "AMAZON_CREATORS_MARKETPLACE": "www.amazon.es",
        "AMAZON_CREATORS_VERSION": "3.2"
      }
    }
  }
}
```

---

## 🔧 Credential Versions Reference

| Version | Auth Architecture | Target Region | Token Endpoint |
| :---: | :---: | :---: | :--- |
| **3.2** | Login with Amazon (LwA) | Europe / Spain | `https://api.amazon.es/auth/o2/token` |
| **3.1** | Login with Amazon (LwA) | North America / US | `https://api.amazon.com/auth/o2/token` |
| **3.3** | Login with Amazon (LwA) | Far East / Japan | `https://api.amazon.co.jp/auth/o2/token` |
| **2.2** | Amazon Cognito | Europe (Legacy) | `https://creatorsapi.auth.eu-south-2.amazoncognito.com/oauth2/token` |
| **2.1** | Amazon Cognito | North America (Legacy) | `https://creatorsapi.auth.us-east-1.amazoncognito.com/oauth2/token` |
| **2.3** | Amazon Cognito | Far East (Legacy) | `https://creatorsapi.auth.us-west-2.amazoncognito.com/oauth2/token` |

---

## 🧪 Testing

The test suite runs against fake HTTP handlers simulating actual API payloads, rate limiting, and token rotations:

```bash
dotnet test AmazonCreators.Net.sln -c Release
```

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

Copyright (c) 2026 **Miguel Molina** ([@molidestroyer](https://github.com/molidestroyer)).
