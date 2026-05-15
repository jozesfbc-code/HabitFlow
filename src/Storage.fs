namespace HabitFlow

open Fable.Core.JsInterop
open Browser

module Storage =
    let private storageKey = "habitflow-data"

    // JSON序列化助手（使用简单手动序列化，避免外部依赖）
    let private habitToJson (h: Habit) : obj =
        createObj [
            "id" ==> (let (HabitId guid) = h.Id in guid.ToString())
            "name" ==> h.Name
            "category" ==> (string h.Category)
            "frequency" ==> (match h.Frequency with
                | Daily -> createObj ["type" ==> "daily"]
                | SpecificDays days -> createObj ["type" ==> "specific"; "days" ==> (days |> List.map string)]
                | TimesPerWeek n -> createObj ["type" ==> "weekly"; "times" ==> n])
            "color" ==> (string h.Color)
            "icon" ==> (string h.Icon)
            "createdAt" ==> h.CreatedAt.ToISOString()
            "checkIns" ==> (h.CheckIns |> List.map (fun d -> d.ToISOString()))
            "archived" ==> h.Archived
        ]

    let private jsonToHabit (json: obj) : Habit option =
        try
            let id = HabitId(System.Guid.Parse(unbox<string> json?id))
            let name = unbox<string> json?name
            let category = 
                match unbox<string> json?category with
                | "Health" -> Health | "Productivity" -> Productivity | "Learning" -> Learning
                | "Creativity" -> Creativity | "Social" -> Social | "Wellness" -> Wellness
                | _ -> Wellness
            let frequencyObj = json?frequency
            let frequency =
                match unbox<string> frequencyObj?``type`` with
                | "daily" -> Daily
                | "specific" -> 
                    let days = unbox<string list> frequencyObj?days
                    SpecificDays (days |> List.map (fun d -> 
                        match d with "Mon"->Mon |"Tue"->Tue |"Wed"->Wed |"Thu"->Thu |"Fri"->Fri |"Sat"->Sat |_ ->Sun))
                | _ -> TimesPerWeek(unbox<int> frequencyObj?times)
            let color = 
                match unbox<string> json?color with
                | "Emerald" -> Emerald | "Blue" -> Blue | "Amber" -> Amber | "Rose" -> Rose
                | "Violet" -> Violet | _ -> Teal
            let icon = 
                match unbox<string> json?icon with
                | "Droplet" -> Droplet | "Book" -> Book | "Dumbbell" -> Dumbbell | "Code" -> Code
                | "Sun" -> Sun | "Moon" -> Moon | "Heart" -> Heart | "Star" -> Star
                | "Zap" -> Zap | _ -> Music
            let createdAt = System.DateTime.Parse(unbox<string> json?createdAt)
            let checkIns = 
                (unbox<string list> json?checkIns) 
                |> List.map System.DateTime.Parse
            let archived = unbox<bool> json?archived
            Some { Id = id; Name = name; Category = category; Frequency = frequency;
                   Color = color; Icon = icon; CreatedAt = createdAt; 
                   CheckIns = checkIns; Archived = archived }
        with _ -> None

    let saveModel (model: Model) : unit =
        let json = createObj [
            "habits" ==> (model.Habits |> List.map habitToJson |> List.toArray)
            "filter" ==> (string model.Filter)
            "sort" ==> (string model.Sort)
        ]
        WebStorage.localStorage.setItem(storageKey, JSON.stringify(json))

    let loadModel () : Model option =
        try
            let jsonStr = WebStorage.localStorage.getItem(storageKey)
            if isNull jsonStr then None
            else
                let json = JSON.parse(jsonStr)
                let habitsJson = unbox<obj array> json?habits
                let habits = habitsJson |> Array.toList |> List.choose jsonToHabit
                Some { Habits = habits
                       Filter = All
                       Sort = ByCreated
                       EditingHabit = None
                       IsAddModalOpen = false
                       SelectedDate = System.DateTime.Now
                       Today = System.DateTime.Now }
        with _ -> None

    let clearStorage () : unit =
        WebStorage.localStorage.removeItem(storageKey)
