{
	if (typeof HTMLElement.prototype.hasFocus == "undefined")
	{
		HTMLElement.prototype.hasFocus = function()
		{
			return this === document.activeElement
		}
	}

	if (typeof HTMLElement.prototype.getSiblingIndex == "undefined")
	{
		HTMLElement.prototype.getSiblingIndex = function()
		{
			return Array.prototype.slice.call(this.parentElement.children).indexOf(this)
		}
	}

	if (typeof HTMLElement.prototype.hasClass == "undefined")
	{
		HTMLElement.prototype.hasClass = function(className)
		{
			if (this.classList)
			{
				return this.classList.contains(className)
			}
			else
			{
				var regex = new RegExp("(\\s|^)" + className + "(\\s|$)")

				return !!this.className.match(regex)
			}
		}
	}

	if (typeof HTMLElement.prototype.addClass == "undefined")
	{
		HTMLElement.prototype.addClass = function(className)
		{
			if (this.classList)
			{
				this.classList.add(className)
			}
			else if (!this.hasClass(className))
			{
				this.className += " " + className
			}

			return this
		}
	}

	if (typeof HTMLElement.prototype.removeClass == "undefined")
	{
		HTMLElement.prototype.removeClass = function(className)
		{
			if (this.classList)
			{
				this.classList.remove(className)
			}
			else if (this.hasClass(className))
			{
				var regex = new RegExp("(\\s|^)" + className + "(\\s|$)")

				this.className = this.className.replace(regex, " ")
			}

			return this
		}
	}

	HTMLElement.prototype.addOrRemoveClass = function(className, add)
	{
		if (add)
		{
			this.addClass(className)
		}
		else
		{
			this.removeClass(className)
		}
	}
}
