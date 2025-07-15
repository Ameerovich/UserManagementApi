using UserManagementApi.Models;

namespace UserManagementApi.Services;

public class UserService
{
    private readonly List<User> _users = new();
    private int _nextId = 1;
    private readonly object _lock = new(); // 🔒 Thread safety for concurrent API calls

    // Returns a shallow copy to protect the internal list from modification
    public List<User> GetAll()
    {
        lock (_lock)
        {
            return _users.ToList(); // Prevents accidental external modification
        }
    }

    public User? GetById(int id)
    {
        lock (_lock)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }
    }

    public User Create(User user)
    {
        lock (_lock)
        {
            user.Id = _nextId++;
            _users.Add(user);
            return user;
        }
    }

    public bool Update(int id, User updatedUser)
    {
        lock (_lock)
        {
            var existing = _users.FirstOrDefault(u => u.Id == id);
            if (existing == null) return false;

            existing.FirstName = updatedUser.FirstName;
            existing.LastName = updatedUser.LastName;
            existing.Email = updatedUser.Email;
            existing.Department = updatedUser.Department;
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null) return false;

            _users.Remove(user);
            return true;
        }
    }
}
