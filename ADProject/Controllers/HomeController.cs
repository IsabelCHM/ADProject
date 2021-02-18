using ADProject.Models;
using ADProject.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ADProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ADProjContext _context;
        private readonly IRecipeService _recipeService;
        private readonly IGroupService _groupService;

        public HomeController(ADProjContext context, IRecipeService recipeService, IGroupService groupService)
        {
            _context = context;
            _recipeService = recipeService;
            _groupService = groupService;
        }

        public async Task<IActionResult> Index(int? pageNumber)
        {

            // Pagination is not implemented in the homepage, but we use it to control how many recipes/groups we want to give to the homepage
            int pageSize = 9;

            var recipeList = await _recipeService.GetAllRecipesQueryable();
            PaginatedList<Recipe> paginatedRecipeList = await PaginatedList<Recipe>.CreateAsync(recipeList, pageNumber ?? 1, pageSize);
            ViewData["recipeList"] = paginatedRecipeList;

            var groupList = await _groupService.GetAllGroupsQueryable();
            PaginatedList<Group> paginatedGroupList = await PaginatedList<Group>.CreateAsync(groupList, pageNumber ?? 1, pageSize);
            ViewData["groupList"] = paginatedGroupList;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult FAQ()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }
    }
}
