{
	if (typeof Array.prototype.first === "undefined")
	{
		Array.prototype.first = function(predicate)
		{
			if (typeof predicate !== "function")
			{
				throw new Error("Argument <predicate> must be a test function.")
			}

			for (var i = 0, len = this.length; i < len; ++i)
			{
				var element = this[i]
				var predicateResult = predicate(element)

				if (typeof predicateResult !== "boolean")
				{
					throw new Error("Argument <predicate> must return a boolean.")
				}

				if (predicateResult)
				{
					return element
				}
			}
		}
	}

	if (typeof Array.prototype.last === "undefined")
	{
		Array.prototype.last = function(predicate)
		{
			if (typeof predicate !== "function")
			{
				throw new Error("Argument <predicate> must be a test function.")
			}

			for (var i = this.length; i > 0; --i)
			{
				var element = this[i - 1]
				var predicateResult = predicate(element)

				if (typeof predicateResult !== "boolean")
				{
					throw new Error("Argument <predicate> must return a boolean.")
				}

				if (predicateResult)
				{
					return element
				}
			}
		}
	}

	if (typeof Array.prototype.where === "undefined")
	{
		Array.prototype.where = function(predicate)
		{
			if (typeof predicate !== "function")
			{
				throw new Error("Argument <predicate> must be a test function.")
			}

			var result = []
			
			for (var i = 0, len = this.length; i < len; ++i)
			{
				var element = this[i]
				var predicateResult = predicate(element)

				if (typeof predicateResult !== "boolean")
				{
					throw new Error("Argument <predicate> must return a boolean.")
				}

				if (predicateResult)
				{
					result.push(element)
				}
			}

			return result
		}
	}

	if (typeof Array.prototype.select === "undefined")
	{
		Array.prototype.select = function(selector)
		{
			if (typeof selector !== "function")
			{
				throw new Error("Argument <selector> must be a transform function.")
			}

			var result = []

			for (var i = 0, len = this.length; i < len; ++i)
			{
				var element = this[i]
				var predicateResult = selector(element)

				result.push(predicateResult)
			}

			return result
		}
	}

	if (typeof Array.prototype.any === "undefined")
	{
		Array.prototype.any = function(predicate)
		{
			return typeof this.first(predicate) !== "undefined"
		}
	}

	if (typeof Array.prototype.isInside === "undefined")
	{
		Array.prototype.isInside = function(arr, predicate)
		{
			if (typeof predicate !== "function")
			{
				throw new Error("Argument <predicate> must be a test function.")
			}

			if (this.length
				&& arr.length)
			{
				for (var i = 0, len = arr.length; i < len; ++i)
				{
					var element = arr[i]

					if (this.any(function(x) { return predicate(element, x) }))
					{
						return true
					}
				}
			}

			return false
		}
	}
}
