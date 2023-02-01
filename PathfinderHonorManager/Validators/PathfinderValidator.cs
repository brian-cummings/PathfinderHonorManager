﻿using System;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Validators
{
    public class PathfinderValidator : AbstractValidator<PathfinderDtoInternal>, IValidator<PathfinderDtoInternal>
    {
        private readonly PathfinderContext _dbContext;

        public PathfinderValidator(PathfinderContext dbContext)
        {
            _dbContext = dbContext;
            SetUpValidation();
        }

        private void SetUpValidation()
        {
            RuleFor(p => p.FirstName).NotEmpty();
            RuleFor(p => p.LastName).NotEmpty();
            RuleFor(p => p.Grade).InclusiveBetween(5, 12);
            RuleSet(
                "post",
                () =>
                {
                    RuleFor(p => p.Email)
                        .EmailAddress()
                        .NotEmpty()
                        .MustAsync(
                            async (email, token) =>
                                !await _dbContext.Pathfinders
                                    .AnyAsync(p => p.Email == email, token))
                        .WithMessage(
                            p => $"Pathfinder email address ({p.Email}) is taken.");
                    RuleFor(p => p.ClubID)
                       .Must(id => id != Guid.Empty)
                       .WithMessage("User must be associated with a valid club before adding a Pathfinder");

                });
        }
    }

}
