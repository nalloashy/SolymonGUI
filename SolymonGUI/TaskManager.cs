public class TaskManager
{
    public List<TaskItem> Tasks { get; } = new List<TaskItem>();

    public void AddTask(string title, string description, DateTime? dueDate = null)
    {
        Tasks.Add(new TaskItem
        {
            Title = title,
            Description = description,
            DueDate = dueDate
        });
    }

    public void CompleteTask(int index)
    {
        if (index >= 0 && index < Tasks.Count)
            Tasks[index].IsCompleted = true;
    }

    public void DeleteTask(int index)
    {
        if (index >= 0 && index < Tasks.Count)
            Tasks.RemoveAt(index);
    }

    public List<TaskItem> GetPendingTasks() =>
        Tasks.Where(t => !t.IsCompleted).ToList();
}