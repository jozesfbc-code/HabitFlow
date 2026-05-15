namespace HabitFlow

open System

// 值对象包装
type HabitId = HabitId of Guid

// 习惯类别
type HabitCategory = 
    | Health 
    | Productivity 
    | Learning 
    | Creativity 
    | Social 
    | Wellness

// 星期
type WeekDay = Mon | Tue | Wed | Thu | Fri | Sat | Sun

// 打卡频率
type Frequency = 
    | Daily 
    | SpecificDays of WeekDay list 
    | TimesPerWeek of int

// 颜色主题
type ColorTheme = Emerald | Blue | Amber | Rose | Violet | Teal

// 图标
type Icon = 
    | Droplet | Book | Dumbbell | Code | Sun | Moon 
    | Heart | Star | Zap | Music

// 习惯实体（不可变Record）
type Habit = {
    Id: HabitId
    Name: string
    Category: HabitCategory
    Frequency: Frequency
    Color: ColorTheme
    Icon: Icon
    CreatedAt: DateTime
    CheckIns: DateTime list
    Archived: bool
}

// 排序选项
type SortOption = ByName | ByCategory | ByStreak | ByCreated

// 筛选选项
type FilterOption = All | Active | Archived | ByCategoryFilter of HabitCategory

// 习惯统计
type HabitStats = {
    CurrentStreak: int
    LongestStreak: int
    CompletionRate: float
    TotalCheckIns: int
    WeeklyProgress: (WeekDay * bool) list
}

// Elmish Model
type Model = {
    Habits: Habit list
    Filter: FilterOption
    Sort: SortOption
    EditingHabit: Habit option
    IsAddModalOpen: bool
    SelectedDate: DateTime
    Today: DateTime
}

// Elmish Message
type Msg =
    // CRUD
    | AddHabit of Habit
    | UpdateHabit of Habit
    | DeleteHabit of HabitId
    | ArchiveHabit of HabitId
    | UnarchiveHabit of HabitId
    // Check-in
    | CheckIn of HabitId * DateTime
    | UncheckIn of HabitId * DateTime
    // UI
    | OpenAddModal
    | CloseAddModal
    | OpenEditModal of HabitId
    | CloseEditModal
    | SetFilter of FilterOption
    | SetSort of SortOption
    | SelectDate of DateTime
    // Storage
    | LoadFromStorage
    | SaveToStorage
    | StorageLoaded of string option
