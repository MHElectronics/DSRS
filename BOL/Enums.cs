namespace BOL
{
    public enum FormMode
    {
        Details,
        Add,
        Edit
    }
    public enum ApprovalStatus
    {
        Prepared = 0,
        Applied = 1,
        Review = 2,
        Approved = 3,
        Rejected = 4,
        Processing = 5,
        Completed = 6
    }
    public enum UserRole
    {
        Admin,
        User
    }
}
