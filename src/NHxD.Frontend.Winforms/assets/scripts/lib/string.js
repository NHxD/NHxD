{
	if (typeof String.prototype.startsWith === "undefined")
	{
		String.prototype.startsWith = function(text)
		{
			return this.substr(0, text.length) === text
		}
	}

	if (typeof String.prototype.endsWith === "undefined")
	{
		String.prototype.endsWith = function(text)
		{
			return this.substr(this.length - text.length) === text
		}
	}

	if (typeof String.prototype.repeat === "undefined")
	{
		String.prototype.repeat = function(count)
		{
			var result = []

			for (var i = 0; i < count; ++i)
			{
				result.push(this)
			}

			return result.join('')
		}
	}

	if (typeof String.prototype.padLeft === "undefined")
	{
		String.prototype.padLeft = function(count, padChar)
		{
			var paddingCount = count - this.length

			if (paddingCount <= 0)
			{
				return this
			}

			return padChar.repeat(paddingCount) + this
		}
	}

	if (typeof String.prototype.splitMultiple === "undefined")
	{
		String.prototype.splitMultiple = function(separators)
		{
			var result = []
			var s = this
			var n = 0

			for (var i = 0, len = this.length; i < len; ++i)
			{
				if (separators.indexOf(this[i]) !== -1)
				{
					if (n > 0)
					{
						result.push(s.substr(0, n))
						s = s.substr(n)
						n = 0
					}
				}
				else
				{
					++n
				}
			}

			if (n > 0)
			{
				result.push(s)
			}

			return result
		}
	}
}
