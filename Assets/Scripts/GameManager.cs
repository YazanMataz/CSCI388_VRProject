using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string[] roomOrder = { "Room1_Switch", "Room2_Sequence", "Room3_HiddenObject", "Room4_Trophy" };
    public static GameManager Instance;
    private int currentRoomIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void StartFixed()
    {
        roomOrder = new string[] { "Room1_Switch", "Room2_Sequence", "Room3_HiddenObject", "Room4_Trophy" };
        currentRoomIndex = 0;
        SceneManager.LoadScene(roomOrder[0]);
    }

    public void StartRandom()
    {
        string[] puzzleRooms = { "Room1_Switch", "Room2_Sequence", "Room3_HiddenObject" };
        // Shuffle
        for (int i = 0; i < puzzleRooms.Length; i++)
        {
            string temp = puzzleRooms[i];
            int rand = Random.Range(i, puzzleRooms.Length);
            puzzleRooms[i] = puzzleRooms[rand];
            puzzleRooms[rand] = temp;
        }
        roomOrder = new string[] { puzzleRooms[0], puzzleRooms[1], puzzleRooms[2], "Room4_Trophy" };
        currentRoomIndex = 0;
        SceneManager.LoadScene(roomOrder[0]);
    }

    public void LoadNextRoom()
    {
        currentRoomIndex++;
        if (currentRoomIndex < roomOrder.Length)
            SceneManager.LoadScene(roomOrder[currentRoomIndex]);
    }
}