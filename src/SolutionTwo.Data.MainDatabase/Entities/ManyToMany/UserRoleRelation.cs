﻿namespace SolutionTwo.Data.MainDatabase.Entities.ManyToMany;

public class UserRoleRelation
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }
}