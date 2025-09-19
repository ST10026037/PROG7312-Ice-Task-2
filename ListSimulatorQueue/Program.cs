using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SimulatorQueue
{
    // A simple class to represent a user with a name and a priority.
    public class User
    {
        public string Name { get; set; }
        public int Priority { get; set; } // Lower number = higher priority

        public User(string name, int priority)
        {
            Name = name;
            Priority = priority;
        }

        public override string ToString()
        {
            return $"[{GetPriorityName()}] {Name}";
        }

        private string GetPriorityName()
        {
            switch (Priority)
            {
                case 1:
                    return "HIGH";
                case 2:
                    return "NORMAL";
                case 3:
                    return "LOW";
                default:
                    return "UNKNOWN";
            }
        }
    }

    class Program
    {
        // Maximum number of users that can be connected at the same time.
        const int MaxConnections = 5;

        // A list that will act as our priority queue.
        static List<User> userQueue = new List<User>();

        // A list to hold users currently connected to the server.
        static List<User> connectedUsers = new List<User>();

        // Random number generator for simulating events.
        static Random random = new Random();

        static void Main(string[] args)
        {
            Console.WriteLine("--- Server User Priority Queue Simulator ---");
            Console.WriteLine($"Maximum number of concurrent connections: {MaxConnections}");
            Console.WriteLine("Priority Levels: 1 (High), 2 (Normal), 3 (Low)");
            Console.WriteLine("Press any key to start the simulation...");
            Console.ReadKey();
            Console.Clear();

            while (true)
            {
                SimulateUserJoining();
                ProcessQueue();
                SimulateUserDisconnecting();
                DisplayStatus();
                Thread.Sleep(random.Next(500, 1500));
            }
        }

        /// <summary>
        /// Simulates a new user attempting to connect, adding them to the priority queue.
        /// </summary>
        static void SimulateUserJoining()
        {
            // A 20% chance for a new user to join the queue.
            if (random.Next(1, 11) <= 2)
            {
                int priority = random.Next(2, 4); // Random priority between Normal (2) and Low (3)
                string newUser = $"User_{Guid.NewGuid().ToString().Substring(0, 4)}";
                userQueue.Add(new User(newUser, priority));
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"✅ {DateTime.Now:HH:mm:ss} | New user added to queue: [{priority}] {newUser}");
                Console.ResetColor();
            }

            // A very small chance for the "King" to join with highest priority.
            if (random.Next(1, 51) == 1)
            {
                User king = new User("The King", 1); // Priority 1 is the highest
                userQueue.Add(king);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"👑 {DateTime.Now:HH:mm:ss} | The King has arrived! Added to queue with highest priority.");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Moves users from the queue to the connected list based on priority.
        /// </summary>
        static void ProcessQueue()
        {
            if (userQueue.Count == 0) return;

            // Sort the queue by priority (lowest number first)
            userQueue = userQueue.OrderBy(u => u.Priority).ToList();

            User userToConnect = userQueue.First();

            // Special logic for the highest priority user
            if (userToConnect.Priority == 1)
            {
                // If the server is full, kick a low-priority user to make space.
                if (connectedUsers.Count >= MaxConnections)
                {
                    User lowestPriorityUser = connectedUsers.OrderByDescending(u => u.Priority).FirstOrDefault();
                    if (lowestPriorityUser != null && lowestPriorityUser.Priority != 1)
                    {
                        connectedUsers.Remove(lowestPriorityUser);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"🚨 {DateTime.Now:HH:mm:ss} | {lowestPriorityUser.Name} was disconnected to make room for a high-priority user!");
                        Console.ResetColor();
                    }
                }
            }

            // Connect the user if a spot is available (or if a spot was just made).
            if (connectedUsers.Count < MaxConnections)
            {
                userQueue.Remove(userToConnect);
                connectedUsers.Add(userToConnect);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"🔌 {DateTime.Now:HH:mm:ss} | {userToConnect.Name} connected to the server!");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Simulates a random user disconnecting from the server.
        /// </summary>
        static void SimulateUserDisconnecting()
        {
            // A 15% chance for a connected user to disconnect.
            if (connectedUsers.Count > 0 && random.Next(1, 11) <= 1.5)
            {
                int userIndex = random.Next(connectedUsers.Count);
                User disconnectedUser = connectedUsers[userIndex];
                connectedUsers.RemoveAt(userIndex);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ {DateTime.Now:HH:mm:ss} | {disconnectedUser.Name} disconnected from the server.");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Displays the current state of the queue and connected users.
        /// </summary>
        static void DisplayStatus()
        {
            Console.WriteLine("\n--- Current Status ---");
            Console.WriteLine($"Connected Users ({connectedUsers.Count}/{MaxConnections}):");
            if (connectedUsers.Count > 0)
            {
                foreach (var user in connectedUsers)
                {
                    Console.WriteLine($"  - {user.Name} (Priority: {user.Priority})");
                }
            }
            else
            {
                Console.WriteLine("  (No users currently connected)");
            }

            Console.WriteLine($"Users in Queue ({userQueue.Count}):");
            if (userQueue.Count > 0)
            {
                foreach (var user in userQueue)
                {
                    Console.WriteLine($"  - {user.Name} (Priority: {user.Priority})");
                }
            }
            else
            {
                Console.WriteLine("  (The queue is empty)");
            }
            Console.WriteLine("----------------------\n");
        }
    }
}