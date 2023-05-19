﻿using System.ComponentModel.DataAnnotations;

namespace CRUDOperationDotNetCoreMVC.Models
{
	public class Movie
	{
		public int Id { get; set; }
		[Required,MaxLength(250)]
		public string Name { get; set; }
		public int Year { get; set; }
		public double Rate { get; set; }
		[Required,MaxLength(2500)]
        public string StoryLine { get; set; }
		[Required]
		public byte[] Poster { get; set; }

        public byte GenreId { get; set; }

		public Genre Genre { get; set; }

    }
}
