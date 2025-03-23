using System.Collections.Generic;

[System.Serializable]
public class ScoreRequest
{
    public int userId;
    public int scoreValue;
    public float roundTime;
    public float averageAccuracy;
    public int moneyEarned;
}

[System.Serializable]
public class ScoreResponse
{
    public int scoreId;
    public int userId;
    public int scoreValue;
    public float roundTime;
    public float averageAccuracy;
    public UserSummary user;
}
[System.Serializable]
public class UserSummary
{
    public int userId;
    public string userName;
    public string playerTag;
}
public class ScoreSummary
{
    public int scoreValue;
    public float roundTime;
    public float averageAccuracy;
    public int moneyEarned;
}
//public class AdminScoreSummary
//{
//    public int scoreId;
//    public int userId;
//    public int scoreValue;
//    public float roundTime;
//    public float averageAccuracy;
//}

[System.Serializable]
public class ScoreResponseList
{
    public List<ScoreResponse> scores;
}