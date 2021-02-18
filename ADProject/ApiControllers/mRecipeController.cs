using ADProject.GenerateTagsClass;
using ADProject.JsonObjects;
using ADProject.Models;
using ADProject.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ADProject.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class mRecipeController : ControllerBase
    {
        private readonly ADProjContext _context;
        private readonly IRecipeService _recipesService;

        public mRecipeController(ADProjContext context, IRecipeService recipeService)
        {
            _context = context;
            _recipesService = recipeService;
        }

        /*[HttpGet]
        [Route("{id}")]
        public ActionResult<Recipe> GetRecipe(int id)
        {
            Recipe recipe = _recipesService.FindRecipeById(id);

            if (recipe == null)
            {
                return null;
            }
            return recipe;
        }*/

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Recipe>> GetRecipeById(int id)
        {
            var recipe = await _recipesService.GetRecipeById(id);

            if (recipe == null)
            {
                return NotFound();
            }

            return recipe;
        }

        [HttpGet]
        [Route("getAllRecipes")]
        public async Task<ActionResult<List<Recipe>>> GetAllRecipes()
        {
            List<Recipe> recipeList = await _recipesService.GetAllRecipesBasic();
            if (recipeList == null)
            {
                return NotFound();
            }

            return recipeList;
        }

        [HttpGet]
        [Route("search/{search}")]
        public async Task<ActionResult<List<Recipe>>> SearchRecipes(string search)
        {
            List<Recipe> recipeList = await _recipesService.GetAllRecipesSearch(search);
            if (recipeList == null)
            {
                return NotFound();
            }

            return recipeList;
        }



        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<Recipe>> CreateNewRecipe([FromBody] RecipePlusTags recipePlusTags)
        {
            Recipe recipe = recipePlusTags.recipe;
            //Recipe recipe = recipePlusTags.recipe;
            string tags = recipePlusTags.tags;
            DateTime now = DateTime.Now;
            recipe.DateCreated = now;
            List<RecipeTag> recipeTags = JsonConvert.DeserializeObject<List<RecipeTag>>(tags);


            /*string[] tags_arr = tags.Replace(" ", "").Split("#");

            foreach (string tag in tags_arr)
            {​​​​
            if (!String.IsNullOrEmpty(tag))
            {​​​​
            Tag t = new Tag
            {​​​​
            TagName = tag,
            Warning = tag
            }​​​​;

            RecipeTag recipeTag = new RecipeTag
            {​​​​
            Tag = t,
            Recipe = recipe
            }​​​​;
            recipeTags.Add(recipeTag);
            }​​​​
            }​​​​*/


            recipe.RecipeTags = recipeTags;

            await _recipesService.AddRecipe(recipe);

            if (recipe == null)
            {
                return NotFound();
            }


            return recipe;
        }
        
        [HttpPost]
        //[Route("post")]
        public async Task<ActionResult<Recipe>> CreateRecipe([FromBody] Recipe recipe)
        {
            try
            {
                //recipe.User = _context.Users.FirstOrDefault();
                DateTime now = DateTime.Now;
                recipe.DateCreated = now;
                await _recipesService.AddRecipe(recipe);
                //_recipesService.AddRecipeNonAsync(recipe);
                
                return recipe;

            } catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
            
        }

        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<Recipe>> CreateNewRecipe([FromBody] RecipePlusTags recipePlusTags)
        {
            Recipe recipe = recipePlusTags.recipe;
            string tags = recipePlusTags.tags;
            DateTime now = DateTime.Now;
            recipe.DateCreated = now;
            List<RecipeTag> recipeTags = JsonConvert.DeserializeObject<List<RecipeTag>>(tags);

            /*string[] tags_arr = tags.Replace(" ", "").Split("#");

            foreach (string tag in tags_arr)
            {
                if (!String.IsNullOrEmpty(tag))
                {
                    Tag t = new Tag
                    {
                        TagName = tag,
                        Warning = tag
                    };

                    RecipeTag recipeTag = new RecipeTag
                    {
                        Tag = t,
                        Recipe = recipe
                    };
                    recipeTags.Add(recipeTag);
                }
            }*/

            recipe.RecipeTags = recipeTags;

            await _recipesService.AddRecipe(recipe);

            if (recipe == null)
            {
                return NotFound();
            }

            return recipe;
        }

        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<booleanJson> DeleteRecipe(int id)
        {
            booleanJson isDeleted = new booleanJson();

            isDeleted.flag = await _recipesService.DeleteRecipe(id);
            return isDeleted;
        }

        [HttpPost]
        [Route("update/{id}")]
        public async Task<booleanJson> UpdateRecipe(int id, [FromBody] RecipePlusTags recipePlusTags)
        {
            Recipe recipe = recipePlusTags.recipe;
            string tags = recipePlusTags.tags;

            List<RecipeTag> recipeTags = JsonConvert.DeserializeObject<List<RecipeTag>>(tags);

            recipe.RecipeTags = recipeTags;

            booleanJson isUpdated = new booleanJson();
            isUpdated.flag = await _recipesService.EditRecipe(id, recipe);
            return isUpdated;
        }

        [HttpPost]
        [Route("generateATags")]
        public async Task<ActionResult<List<RecipeTag>>> GenerateAllergenTags([FromBody] List<RecipeIngredient> recipeIngredients)
        {
            Debug.Write("Reached generate A Tags");

            GenerateTag trial = new GenerateTag(_recipesService);

            string allergens = trial.GetAllergenTag(recipeIngredients);

            List<RecipeTag> tags = new List<RecipeTag>();

            tempAllergenTags tempAlTags = JsonConvert.DeserializeObject<tempAllergenTags>(allergens);
            if (tempAlTags.allergens != null)
            {
                Debug.WriteLine(tempAlTags.allergens[0]);
                for (int i = 0; i < tempAlTags.allergens.Count; i++)
                {
                    tags.Add(new RecipeTag
                    {
                        IsAllergenTag = true,
                        Tag = new Tag
                        {
                            TagName = tempAlTags.allergens[i],
                            Warning = tempAlTags.allergens[i]
                        }
                    });
                }
            }

            return tags;
        }

        /*[HttpDelete]
        [Route("deleterecipe/{​​id}​​")]
        public async Task<ActionResult<booleanJson> DeleteRecipe(int id)
        {​​
            booleanJson sample = new booleanJson();
            try
            {​​
                await _recipesService.DeleteRecipe(id);
                sample.flag = true;
                return sample;
            }​​ catch
            {​​
                sample.flag = false;
                return sample;
            }​​
        }*/


    }
}
