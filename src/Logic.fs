namespace HabitFlow

open System

module Logic =
    // ===== 习惯CRUD =====
    let createHabit (name: string) (category: HabitCategory) (frequency: Frequency) (color: ColorTheme) (icon: Icon) : Habit =
        { Id = HabitId(Guid.NewGuid())
          Name = name
          Category = category
          Frequency = frequency
          Color = color
          Icon = icon
          CreatedAt = DateTime.Now
          CheckIns = []
          Archived = false }

    let updateHabitName (habit: Habit) (newName: string) : Habit =
        { habit with Name = newName }

    let toggleArchive (habit: Habit) : Habit =
        { habit with Archived = not habit.Archived }

    let checkIn (habit: Habit) (date: DateTime) : Habit =
        // 如果该日期已存在则不重复添加
        let alreadyChecked = 
            habit.CheckIns |> List.exists (fun d -> d.Date = date.Date)
        if alreadyChecked then habit
        else { habit with CheckIns = date :: habit.CheckIns }

    let uncheckIn (habit: Habit) (date: DateTime) : Habit =
        { habit with CheckIns = habit.CheckIns |> List.filter (fun d -> d.Date <> date.Date) }

    // ===== 日期工具 =====
    let isSameDay (d1: DateTime) (d2: DateTime) : bool =
        d1.Date = d2.Date

    let getWeekDay (date: DateTime) : WeekDay =
        match date.DayOfWeek with
        | DayOfWeek.Monday -> Mon
        | DayOfWeek.Tuesday -> Tue
        | DayOfWeek.Wednesday -> Wed
        | DayOfWeek.Thursday -> Thu
        | DayOfWeek.Friday -> Fri
        | DayOfWeek.Saturday -> Sat
        | DayOfWeek.Sunday -> Sun
        | _ -> failwith "Invalid day of week"

    let getDayAbbrev = function
        | Mon -> "Mon" | Tue -> "Tue" | Wed -> "Wed" | Thu -> "Thu"
        | Fri -> "Fri" | Sat -> "Sat" | Sun -> "Sun"

    let getWeekRange (date: DateTime) : (DateTime * DateTime) =
        let dayOfWeek = int date.DayOfWeek
        let adjusted = if dayOfWeek = 0 then 6 else dayOfWeek - 1  // Mon=0
        let monday = date.AddDays(-float adjusted)
        let sunday = monday.AddDays(6.0)
        (monday, sunday)

    let formatDate (date: DateTime) : string =
        date.ToString("MMM dd, yyyy")

    // ===== 统计计算 =====
    // 计算到指定日期的连续打卡天数
    let calculateStreak (habit: Habit) (asOf: DateTime) : int =
        if habit.CheckIns.IsEmpty then 0
        else
            let sorted = habit.CheckIns |> List.map (fun d -> d.Date) |> List.distinct |> List.sortDescending
            let rec countStreak (dates: DateTime list) (expectedDate: DateTime) (count: int) : int =
                match dates with
                | [] -> count
                | d :: rest ->
                    if d = expectedDate then countStreak rest (expectedDate.AddDays(-1.0)) (count + 1)
                    else count
            // 如果今天没打卡，从昨天开始算
            let latest = sorted |> List.head
            if latest = asOf.Date then countStreak sorted asOf.Date 0
            elif latest = asOf.Date.AddDays(-1.0) then countStreak sorted (asOf.Date.AddDays(-1.0)) 0
            else 0

    let calculateLongestStreak (habit: Habit) : int =
        if habit.CheckIns.IsEmpty then 0
        else
            let sorted = habit.CheckIns |> List.map (fun d -> d.Date) |> List.distinct |> List.sort
            let rec findStreaks (dates: DateTime list) (currentStreak: int) (maxStreak: int) : int =
                match dates with
                | [] -> max currentStreak maxStreak
                | [d] -> max (currentStreak + 1) maxStreak
                | d1 :: d2 :: rest ->
                    if (d2 - d1).Days = 1 then
                        findStreaks (d2 :: rest) (currentStreak + 1) maxStreak
                    else
                        findStreaks (d2 :: rest) 0 (max (currentStreak + 1) maxStreak)
            findStreaks sorted 0 0

    let calculateCompletionRate (habit: Habit) (asOf: DateTime) : float =
        if habit.CheckIns.IsEmpty then 0.0
        else
            let totalDays = max ((asOf.Date - habit.CreatedAt.Date).Days + 1) 1
            let checkedDays = habit.CheckIns |> List.map (fun d -> d.Date) |> List.distinct |> List.length
            float checkedDays / float totalDays

    let getWeeklyProgress (habit: Habit) (asOf: DateTime) : (WeekDay * bool) list =
        let (monday, _) = getWeekRange asOf
        [ for i in 0..6 do
            let date = monday.AddDays(float i)
            let weekDay = getWeekDay date
            let isChecked = habit.CheckIns |> List.exists (fun d -> d.Date = date.Date)
            (weekDay, isChecked) ]

    let getStats (habit: Habit) (asOf: DateTime) : HabitStats =
        { CurrentStreak = calculateStreak habit asOf
          LongestStreak = calculateLongestStreak habit
          CompletionRate = calculateCompletionRate habit asOf
          TotalCheckIns = habit.CheckIns |> List.distinctBy (fun d -> d.Date) |> List.length
          WeeklyProgress = getWeeklyProgress habit asOf }

    // ===== 筛选与排序（高阶函数）=====
    let filterHabits (filter: FilterOption) (habits: Habit list) : Habit list =
        match filter with
        | All -> habits
        | Active -> habits |> List.filter (fun h -> not h.Archived)
        | Archived -> habits |> List.filter (fun h -> h.Archived)
        | ByCategoryFilter cat -> habits |> List.filter (fun h -> h.Category = cat && not h.Archived)

    let sortHabits (sort: SortOption) (habits: Habit list) : Habit list =
        match sort with
        | ByName -> habits |> List.sortBy (fun h -> h.Name.ToLower())
        | ByCategory -> habits |> List.sortBy (fun h -> h.Category)
        | ByCreated -> habits |> List.sortByDescending (fun h -> h.CreatedAt)
        | ByStreak -> 
            let now = DateTime.Now
            habits |> List.sortByDescending (fun h -> calculateStreak h now)

    let getTodaysHabits (today: DateTime) (habits: Habit list) : Habit list =
        habits 
        |> List.filter (fun h -> not h.Archived)
        |> List.filter (fun h ->
            match h.Frequency with
            | Daily -> true
            | SpecificDays days -> days |> List.contains (getWeekDay today)
            | TimesPerWeek _ -> true)

    // ===== 导出/导入 =====
    let exportData (model: Model) : string =
        // 简单序列化：习惯名列表和打卡次数
        model.Habits 
        |> List.map (fun h -> sprintf "%s (%d check-ins)" h.Name h.CheckIns.Length)
        |> String.concat "\n"
