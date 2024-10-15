public class LeaderboardModel
{
    public string Username { get; set; }
    public int TotalPoints { get; set; }
    public string GUID { get; set; }
    public List<RouteInfo> FinishedRoutes { get; set; }  // List to hold route details
}

public class RouteInfo
{
    public string RouteID { get; set; }
    public string ImageFileId { get; set; }
    public double Distance { get; set; }
    public double Co2Saved { get; set; }
}
