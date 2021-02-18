using ADProject.Models;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MoreLinq.Extensions.DistinctByExtension;

namespace ADProject.Service
{
    public class GroupService : IGroupService
    {
        private readonly ADProjContext _context;

        public GroupService(ADProjContext context)
        {
            _context = context;
        }

        public async Task<bool> AddGroup(Group group)
        {
            group.UsersGroups = await this.CheckUsernameExist(group.UsersGroups.ToList());
            group.GroupTags = await this.CheckTagsDatabase(group.GroupTags.ToList());
            _context.Add(group);
            var saveResult = await _context.SaveChangesAsync();
            return saveResult >= 1;
        }

        public async Task<bool> DeleteGroup(int id)
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.GroupId == id);
            _context.Groups.Remove(group);
            var saveResult = await _context.SaveChangesAsync();
            return saveResult >= 1;
        }

        public async Task<bool> EditGroup(int id, Group group)
        {
            try
            {
                var dbGroup = await _context.Groups
                    .Include(g => g.GroupTags)
                    .ThenInclude(gt => gt.Tag)
                    .Include(g => g.RecipeGroups)
                    .ThenInclude(rg => rg.Recipe)
                    .Include(g => g.UsersGroups)
                    .ThenInclude(ug => ug.User)
                    .FirstOrDefaultAsync(g => g.GroupId == id);

                var newUsersGroup = await this.CheckUsernameExistEditVer(group.UsersGroups, id);

                _context.GroupTags.RemoveRange(dbGroup.GroupTags);
                _context.RecipeGroups.RemoveRange(dbGroup.RecipeGroups);
                _context.UsersGroups.RemoveRange(dbGroup.UsersGroups);

                dbGroup.GroupName = group.GroupName;

                if(group.GroupPhoto != "")

                {
                    dbGroup.GroupPhoto = group.GroupPhoto;
                }

                dbGroup.Description = group.Description;
                dbGroup.IsPublished = group.IsPublished;
                dbGroup.RecipeGroups = group.RecipeGroups;

                dbGroup.GroupTags = await this.CheckTagsDatabase(group.GroupTags);
                dbGroup.UsersGroups = newUsersGroup;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<List<Group>> GetAllGroups()
        {
            return await _context.Groups
                .Include(g => g.GroupTags)
                .ThenInclude(gt => gt.Tag)
                .Include(g => g.RecipeGroups)
                .ThenInclude(rg => rg.Recipe)
                .Include(g => g.UsersGroups)
                .ThenInclude(ug => ug.User)
                .ToListAsync();
        }

        public async Task<List<Group>> GetAllGroups1()
        {
            return await _context.Groups
                .ToListAsync();
        }

        public async Task<IQueryable<Group>> GetAllGroupsQueryable()
        {
            var groups = await this.GetAllGroups();
            return groups.AsQueryable();
        }

        public async Task<IQueryable<UsersGroup>> GetMyGroups(int id)
        {
            return _context.UsersGroups
                .Where(ug => ug.UserId == id)
                .Include(ug => ug.Group)
                .ThenInclude(g => g.GroupTags)
                .ThenInclude(gt => gt.Tag)
                .Include(ug => ug.User)
                .AsQueryable();
        }

        public async Task<IQueryable<UsersGroup>> GetMyGroupsSearch(int id, string search)
        {
            var myGroups = await this.GetMyGroups(id);
            return myGroups.Where(ug =>
                ug.Group.GroupName.Contains(search) ||
                ug.Group.Description.Contains(search) ||
                ug.Group.GroupTags.Any(gt => gt.Tag.TagName.Contains(search)));
        }

        //TODO: Refactor this if there is time as it is really slow
        public async Task<Group> GetGroupById(int? id)
        {
            return await _context.Groups
                .Include(g => g.GroupTags)
                .ThenInclude(gt => gt.Tag)

                .Include(g => g.RecipeGroups)
                .ThenInclude(rg => rg.Recipe)
                .ThenInclude(r => r.RecipeTags)
                .ThenInclude(rt => rt.Tag)
                
                .Include(g => g.RecipeGroups)
                .ThenInclude(rg => rg.Recipe)
                .ThenInclude(r => r.RecipeIngredients)

                .Include(g => g.RecipeGroups)
                .ThenInclude(rg => rg.Recipe)
                .ThenInclude(r => r.SavedRecipes)
                .ThenInclude(sr => sr.User)

                .Include(g => g.RecipeGroups)
                .ThenInclude(rg => rg.Recipe)
                .ThenInclude(r => r.User)

                .Include(g => g.UsersGroups)
                .ThenInclude(ug => ug.User)

                .FirstOrDefaultAsync(g => g.GroupId == id);
        }


        public async Task<Group> ADGetGroupById(int? id)
        {
            Group group =  await _context.Groups
                .Include(g => g.GroupTags)
                .ThenInclude(gt => gt.Tag)
                .Include(g => g.RecipeGroups)
                .ThenInclude(rg => rg.Recipe)
                .ThenInclude(r => r.User)
                .Include(g => g.RecipeGroups)
                .ThenInclude(rg => rg.Recipe)
                .ThenInclude(rt => rt.RecipeTags)
                .ThenInclude(t => t.Tag)
                .FirstOrDefaultAsync(g => g.GroupId == id);

            foreach (RecipeGroup r in group.RecipeGroups)
            {
                ApplicationUser n = new ApplicationUser
                {
                    UserName = r.Recipe.User.UserName
                };

                r.Recipe.User = n;

            }

            foreach (RecipeGroup rg in group.RecipeGroups)
            {
                foreach (RecipeTag rt in rg.Recipe.RecipeTags)
                {
                    rt.Recipe = null;
                }
            }

            return group;
        }

        public async Task<List<Group>> GetAllGroupsSearch(string search)
        {
            List<Group> gList = await _context.Groups
                .Include(r => r.GroupTags)
                .ThenInclude(rtag => rtag.Tag)
                .Where(r => r.GroupName.Contains(search)
                            || r.Description.Contains(search)
                            || r.GroupTags.Any(y => y.Tag.TagName.Contains(search)))
                .ToListAsync();

            /*foreach (Recipe r in rList)
            {
                User n = new User
                {
                    UserId = r.User.UserId,
                    Username = r.User.Username
                };

                r.User = n;
            }*/

            return gList;

        }


        public async Task<IQueryable<Group>> GetAllGroupsSearchQueryable(string search)
        {
            var groups = await this.GetAllGroupsSearch(search);
            return groups.AsQueryable();
        }

        // Check if the username exist in database
        private async Task<List<UsersGroup>> CheckUsernameExist(List<UsersGroup> usersGroup)
        {
            List<UsersGroup> foundUsers = new List<UsersGroup>();

            for(int i = 0; i < usersGroup.Count; i++)
            {
                if(usersGroup[i].User.UserName != null)
                {
                    var existUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == usersGroup[i].User.UserName.ToLower().Trim());
                    if(existUser != null && usersGroup[i].IsMod == true)
                    {
                        foundUsers.Add(usersGroup[i]);
                    }
                    else if (existUser != null)
                    {
                        foundUsers.Add(new UsersGroup
                        {
                            User = existUser,
                            UserId = existUser.UserId,
                            IsMod = false
                        });
                    }
                }
            }

            return foundUsers.DistinctBy(u => u.User.Id).ToList();
        }

        private async Task<List<UsersGroup>> CheckUsernameExistEditVer(List<UsersGroup> inputUsersGroup, int groupId)
        {
            List<UsersGroup> foundUsers = new List<UsersGroup>();
            List<UsersGroup> usersGroup = inputUsersGroup.DistinctBy(u => u.User.UserName).ToList();

            for (int i = 0; i < usersGroup.Count; i++)
            {
                if (usersGroup[i].User.UserName != null)
                {
                    var existUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == usersGroup[i].User.UserName.ToLower().Trim());
                    if (existUser != null)
                    {
                        if (await this.IsGroupAdmin(groupId, existUser.UserName) == true)
                        {
                            foundUsers.Add(new UsersGroup
                            {
                                UserId = existUser.Id,
                                User = existUser,
                                IsMod = true,
                            });
                        }
                        else
                        {
                            foundUsers.Add(new UsersGroup
                            {
                                UserId = existUser.Id,
                                User = existUser,
                                IsMod = false,
                            });
                        }
                    }
                }
            }

            return foundUsers;
        }

        // Check if the tag exist in database
        private async Task<List<GroupTag>> CheckTagsDatabase(List<GroupTag> groupTags)
        {
            for (int i = 0; i < groupTags.Count(); i++)
            {
                if (groupTags[i].Tag.TagName != null)
                {
                    var existTag = await _context.Tags.FirstOrDefaultAsync(t => t.TagName.ToLower() == groupTags[i].Tag.TagName.ToLower().Trim());
                    if (existTag != null)
                    {
                        groupTags[i].TagId = existTag.TagId;
                        groupTags[i].Tag = existTag;
                    }
                    else
                    {
                        groupTags[i].Tag.Warning = groupTags[i].Tag.TagName;
                    }
                }
            }

            return groupTags.FindAll(gt => gt.Tag.TagName != null).DistinctBy(gt => gt.Tag.TagName).ToList();
        }


        public async Task<Group> AddGroupAD(Group group)
        {
            //group.UsersGroups = await this.CheckUsernameExist(group.UsersGroups.ToList());
            group.GroupTags = await this.CheckTagsDatabase(group.GroupTags.ToList());
            _context.Add(group);
            var saveResult = await _context.SaveChangesAsync();

            Group rg = _context.Groups.Where(x => x.GroupName == group.GroupName).FirstOrDefault();

            return rg;
            
        }

        public List<Group> UserInGroups(int userId)
        {
            List<Group> groups = _context.UsersGroups
                .Where(ug => ug.User.Id == userId)
                .Select(ug => ug.Group)
                .ToList();

            return groups;
        }

        public List<Group> RecipeInGroups(int recipeId)
        {
            List<Group> groups = _context.RecipeGroups
                .Where(ug => ug.Recipe.RecipeId == recipeId)
                .Select(ug => ug.Group)
                .ToList();

            return groups;
        }

        public async Task<bool> PostRecipes(List<Group> groups, int recipeId)
        {
            foreach (Group g in groups)
            {
                RecipeGroup rg = new RecipeGroup
                {
                    GroupId = g.GroupId,
                    RecipeId = recipeId
                };

                _context.Add(rg);

            }

            var saveResult = await _context.SaveChangesAsync();
            return saveResult >= 1;
        }


        public async Task<bool> IsGroupAdmin(int? groupId, string username)
        {
            if(groupId == null)
            {
                return false;
            }

            var group = await _context.Groups
                .Include(g => g.UsersGroups)
                .ThenInclude(ug => ug.User)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if(group == null)
            {
                return false;
            }

            foreach(var ug in group.UsersGroups)
            {
                if (ug.User.UserName.Equals(username) && ug.IsMod == true)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> IsGroupMember(int? groupId, string username)
        {
            if (groupId == null)
            {
                return false;
            }

            var group = await _context.Groups
                .Include(g => g.UsersGroups)
                .ThenInclude(ug => ug.User)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null)
            {
                return false;
            }

            foreach (var ug in group.UsersGroups)
            {
                if (ug.User.UserName.Equals(username))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> JoinGroupWebVer(int? groupId, string username)
        {
            if (groupId == null || username == null)
            {
                return false;
            }

            var group = await _context.Groups
                .Include(g => g.UsersGroups)
                .ThenInclude(ug => ug.User)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null)
            {
                return false;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName.Equals(username));

            if(user == null || await this.IsGroupMember(groupId, username))
            {
                return false;
            }

            try
            {
                group.UsersGroups.Add(new UsersGroup
                {
                    UserId = user.Id,
                    GroupId = group.GroupId,
                    User = user,
                    Group = group,
                    IsMod = false
                });

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> LeaveGroupWebVer(int? groupId, string username)
        {
            if(groupId == null || username == null || !await this.IsGroupMember(groupId, username))
            {
                return false;
            }

            var usergroup = await _context.UsersGroups.FirstOrDefaultAsync(ug => ug.User.UserName == username && ug.GroupId == groupId);

            if(usergroup == null)
            {
                return false;
            }

            _context.UsersGroups.Remove(usergroup);
            var success = await _context.SaveChangesAsync();
            return success >= 1;
        }


        public async Task<IQueryable<RecipeGroup>> getRecipesGroupByGroupId(int? groupId)
        {
            var recipeGroup = await _context.RecipeGroups
                    .Include(r => r.Recipe)
                    .ThenInclude(r=>r.RecipeIngredients)
                    .Include(r => r.Recipe)
                    .ThenInclude(r => r.RecipeTags)
                    .ThenInclude(rt => rt.Tag)
                    .Where(u => u.GroupId == groupId)
                    .ToListAsync();
            return recipeGroup.AsQueryable();
        }
        public async Task<IQueryable<RecipeGroup>> getRecipesGroupSearchByGroupId(int? groupId, string search)
        {
            var recipeGroup =await  this.getRecipesGroupByGroupId(groupId);
            return recipeGroup.Where(sr => sr.Recipe.Title.Contains(search) ||
                      sr.Recipe.Description.Contains(search) ||
                      sr.Recipe.RecipeIngredients.Any(y => y.Ingredient.Contains(search)) ||
                      sr.Recipe.RecipeTags.Any(y => y.Tag.TagName.Contains(search)))
                .AsQueryable();

        }

        public async Task<bool> IsGroupMemberAD(int groupId, int userId)
        {
            var group = await _context.Groups
                .Include(g => g.UsersGroups)
                .ThenInclude(ug => ug.User)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null)
            {
                return false;
            }

            foreach (var ug in group.UsersGroups)
            {
                if (ug.UserId == userId)
                {
                    return true;
                }
            }

            return false;

        }


    }
}
