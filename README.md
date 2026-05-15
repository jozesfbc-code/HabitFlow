# HabitFlow

> A beautiful, functional habit tracker built with **F#**, **Elmish**, and **Feliz**. Track your daily habits, visualize your progress, and build consistency — all powered by functional programming principles.

[![F#](https://img.shields.io/badge/F%23-8.0-378BBA?logo=fsharp)](https://fsharp.org/)
[![Elmish](https://img.shields.io/badge/Elmish-MVU-60B5CC)](https://elmish.github.io/)
[![Feliz](https://img.shields.io/badge/Feliz-React%20DSL-61DAFB?logo=react)](https://zaid-ajaj.github.io/Feliz/)
[![Fable](https://img.shields.io/badge/Fable-4.x-yellowgreen)](https://fable.io/)

---

## Screenshots

### Dashboard Overview
![Dashboard](screenshots/dashboard.png)

### Add/Edit Habit Modal
![Add Habit](screenshots/add-habit.png)

### Weekly Progress Tracking
![Weekly Progress](screenshots/weekly-progress.png)

---

## Try It Live

**[https://jozesfbc-code.github.io/HabitFlow](https://jozesfbc-code.github.io/HabitFlow)**

Replace `jozesfbc-code` with your actual GitHub username after forking and deploying.

---

## Motivation

HabitFlow was created as the **Omega Project** for a Functional Programming course, with the goal of demonstrating how functional programming concepts — particularly those in F# — can be applied to build real-world, interactive web applications.

Unlike traditional habit trackers built with imperative frameworks, HabitFlow showcases:

- **Immutable state management** through Elmish's Model-View-Update architecture
- **Pure functions** for all business logic — predictable, testable, and side-effect-free
- **Pattern matching** for exhaustive state handling with compile-time safety
- **Function composition** and the pipeline operator for elegant data transformations
- **Algebraic Data Types** (Records and Discriminated Unions) for domain modeling

---

## Features

| Feature | Description |
|---------|-------------|
| **Habit CRUD** | Create, read, update, archive, and delete habits |
| **Daily Check-in** | One-tap check-in with visual feedback and animations |
| **Streak Tracking** | Automatic calculation of current and longest streaks |
| **Weekly Progress** | Visual 7-day progress bar for each habit |
| **Statistics Dashboard** | Overview of total habits, today's completion, average streak, total check-ins |
| **Filtering & Sorting** | Filter by category or archive status; sort by name, category, streak, or creation date |
| **Persistent Storage** | All data saved to browser's localStorage |
| **Responsive Design** | Works on desktop, tablet, and mobile devices |
| **Beautiful UI** | Warm, low-saturation color palette with smooth animations |

---

## Tech Stack

| Technology | Purpose |
|-----------|---------|
| **F#** | Primary programming language — functional-first, type-safe |
| **Fable** | F# to JavaScript compiler — brings F# to the browser |
| **Elmish** | Model-View-Update (MVU) architecture — state management |
| **Feliz** | Type-safe, functional React DSL for F# |
| **Vite** | Fast development server and production bundler |
| **CSS3** | Custom styles with CSS variables and keyframe animations |

---

## Functional Programming Highlights

This project demonstrates the following F# functional programming concepts:

| Concept | Application |
|---------|-------------|
| **Immutable Records** | All state (`Model`, `Habit`, `HabitStats`) is immutable — updates create new instances |
| **Discriminated Unions** | `Msg`, `HabitCategory`, `WeekDay`, `Frequency`, `FilterOption` — exhaustive pattern matching |
| **Pattern Matching** | `update` function handles all 17 `Msg` cases with compile-time exhaustiveness checking |
| **Pipeline Operator `\|>`** | Fluent data transformations throughout `Logic.fs` and `View.fs` |
| **Function Composition** | `filterHabits >> sortHabits` composable data processing pipeline |
| **Higher-Order Functions** | `List.map`, `List.filter`, `List.fold`, `List.partition`, `List.sortBy` |
| **Option&lt;'T&gt;** | `EditingHabit: Habit option` eliminates null reference errors |
| **Pure Functions** | `Logic.fs` contains zero side effects — same input always produces same output |
| **Recursive Functions** | `calculateStreak` uses tail-recursive style for consecutive day counting |
| **Record Copy-and-Update** | `{ model with Habits = newHabits }` syntax for immutable state transitions |
| **Type Inference** | Minimal type annotations — compiler infers most types automatically |

---

## Project Structure

```
HabitFlow/
 src/
   Types.fs       -- Domain models (immutable Records, DUs)
   Logic.fs       -- Pure business logic (streaks, stats, filters)
   Storage.fs     -- localStorage persistence (JSON ser/de)
   State.fs       -- Elmish: Model, init, update
   View.fs        -- Feliz: all UI components
   App.fs         -- Entry point: Program.mkProgram
 public/
   index.html     -- HTML entry point
   style.css      -- Custom styles & animations
 .github/workflows/
   deploy.yml     -- GitHub Actions: auto-deploy to Pages
 HabitFlow.fsproj -- F# project file
 package.json     -- npm dependencies
 vite.config.js   -- Vite bundler config
```

---

## How to Build & Run

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)

### Setup

```bash
# Clone the repository
git clone https://github.com/jozesfbc-code/HabitFlow.git
cd HabitFlow

# Install .NET tools (Fable compiler)
dotnet tool restore

# Restore NuGet packages
dotnet restore

# Install npm dependencies
npm install
```

### Development

```bash
# Run in development mode with hot reloading
dotnet fable watch --run vite

# Or separately:
dotnet fable watch  # Terminal 1: compile F# to JS
npm run dev         # Terminal 2: start Vite dev server
```

Then open [http://localhost:3000](http://localhost:3000) in your browser.

### Production Build

```bash
# Compile F# and bundle for production
dotnet fable
npm run build

# Output will be in the `dist/` directory
```

---

## Deployment

This project is configured for automatic deployment to **GitHub Pages** via GitHub Actions:

1. Fork this repository
2. Go to **Settings → Pages** in your fork
3. Set **Source** to "GitHub Actions"
4. Push to the `main` branch — the workflow will build and deploy automatically

Your app will be available at `https://jozesfbc-code.github.io/HabitFlow`

---

## Architecture Overview

HabitFlow follows the **Elmish Model-View-Update (MVU)** architecture:

```
         User Interaction
               │
               ▼
            ┌─────────┐
            │   Msg   │  ← Discriminated Union: AddHabit | CheckIn | SetFilter | ...
            └────┬────┘
                 │
                 ▼
         ┌───────────────┐
         │ update: Msg   │  ← Pure function (no side effects!)
         │  → Model      │    Pattern match on Msg, return new Model
         └───────┬───────┘
                 │
                 ▼
         ┌───────────────┐     ┌─────────────┐
         │ Storage.save  │────→│ localStorage │  ← Side effects via Cmd
         └───────┬───────┘     └─────────────┘
                 │
                 ▼
         ┌───────────────┐
         │ View.render   │  ← Pure function: Model → ReactElement
         └───────┬───────┘
                 │
                 ▼
              ┌──────┐
              │  DOM  │  ← React renders the updated UI
              └──────┘
```

**Key principle**: The `update` function is completely pure — it never touches localStorage or the DOM directly. All side effects are handled through Elmish's `Cmd` (Command) system, which schedules effects outside the pure update cycle.

---

## License

MIT License — feel free to use, modify, and distribute.

---

> *Built with love for functional programming and the F# community.*
