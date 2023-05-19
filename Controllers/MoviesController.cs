using CRUDOperationDotNetCoreMVC.Models;
using CRUDOperationDotNetCoreMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace CRUDOperationDotNetCoreMVC.Controllers
{
	public class MoviesController : Controller
	{
		private readonly AppDbContext _context;
		private new List<string> _allowedExtentions = new List<string> { ".jpg", ".png" };
		private readonly IToastNotification _toastNotification;
		private long _maxAllowedPosterSize = 1048576;
		public MoviesController(AppDbContext context,IToastNotification toastNotification)
        {
			_context = context;
			_toastNotification = toastNotification;
        }
        public async Task<IActionResult> Index()
		{
			var movies = await _context.Movies.OrderByDescending(m=>m.Rate).ToListAsync();

			return View(movies);
		}
		public async Task<IActionResult> Create()
		{
			var viewModel = new MovieFormViewModel
			{
				Genres = await _context.Genres.OrderBy(m=>m.Name).ToListAsync()
			};
			return View("MovieForm",viewModel);
		}
	    [HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(MovieFormViewModel model)
		{	
			// check for model state is valid

			if(!ModelState.IsValid)
			{
				model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
				return View("MovieForm", model);
            }

			// check for there is a file

			var files = Request.Form.Files;


			if(!files.Any())
			{
				model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
				ModelState.AddModelError("Poster", "Please Select movie Poster");
				return View("MovieForm", model);
			}
			
			// check for the extension of the file

			var poster = files.FirstOrDefault();

			if(!_allowedExtentions.Contains(Path.GetExtension(poster.FileName).ToLower()))
			{
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster","Only .png , jpg images are allowed");
                return View("MovieForm", model);
            }

			// check for the size of the file 1048576 byte == 1 Mega

			if (poster.Length > _maxAllowedPosterSize)
			{
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "poster cannot be > 1MB");
                return View("MovieForm", model);
            }

			//save the record in database

			using var dataStream=new MemoryStream();

			await poster.CopyToAsync(dataStream);

			//we can use package (automapper) to map movieFormViewModel to Movie model
			// to store in database

			var movie = new Movie
			{
				Name = model.Name,
				GenreId=model.GenreId,
				Year=model.Year,
				Rate=model.Rate,
				StoryLine=model.StoryLine,
				Poster=dataStream.ToArray()

			};
			_context.Movies.Add(movie);
			_context.SaveChanges();
			_toastNotification.AddSuccessToastMessage("Movie created successfully") ;

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return BadRequest();

			var movie = await _context.Movies.FindAsync(id);

			if (movie == null) return NotFound();

			var viewModel = new MovieFormViewModel
			{
				Id = movie.Id,
				Name = movie.Name,
				GenreId = movie.GenreId,
				Rate = movie.Rate,
				Year = movie.Year,
				StoryLine = movie.StoryLine,
				Poster = movie.Poster,
				Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync()
			};

			return View("MovieForm", viewModel);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(MovieFormViewModel model)
		{
			// check for model state is valid

			if (!ModelState.IsValid)
			{
				model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
				return View("MovieForm", model);
			}
			var movie = await _context.Movies.FindAsync(model.Id);

			if (movie == null) return NotFound();

			var files = Request.Form.Files;

			if (files.Any())
			{
				var poster = files.FirstOrDefault();

				using var dataStream = new MemoryStream();

				await poster.CopyToAsync(dataStream);

				model.Poster = dataStream.ToArray();

				if (!_allowedExtentions.Contains(Path.GetExtension(poster.FileName).ToLower()))
				{
					model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
					ModelState.AddModelError("Poster", "Only .png , jpg images are allowed");
					return View("MovieForm", model);
				}

				// check for the size of the file 1048576 byte == 1 Mega

				if (poster.Length > _maxAllowedPosterSize)
				{
					model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
					ModelState.AddModelError("Poster", "poster cannot be > 1MB");
					return View("MovieForm", model);
				}
				movie.Poster = model.Poster;

			}

			movie.Name = model.Name;
			movie.GenreId = model.GenreId;
			movie.Year = model.Year;
			movie.Rate = model.Rate;
			movie.StoryLine = model.StoryLine;

			_context.SaveChanges();
			_toastNotification.AddSuccessToastMessage("Movie Updated successfully");

			return RedirectToAction(nameof(Index));

		}
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return BadRequest();

			var movie = await _context.Movies.Include(m=>m.Genre).SingleOrDefaultAsync(m=>m.Id==id);

			if (movie == null) return NotFound();

			return View(movie);

		}
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return BadRequest();

			var movie = await _context.Movies.FindAsync(id);

			if (movie == null) return NotFound();

			_context.Movies.Remove(movie);
			_context.SaveChanges();

			//var movies = await _context.Movies.OrderByDescending(m => m.Rate).ToListAsync();

			//return View("Index",movies);

			return Ok();

		}
	}
}
