{
	// require HTML elements with those following id's:
	// - "page-index-input"
	// - "nav-button-previous"
	// - "nav-button-next"
	// - "nav-button-first"
	// - "nav-button-last"
	// - "nav-button-top"
	// - "nav-button-bottom"
	// - "header"

	var Navigation = {}

	Navigation.input = null
	Navigation.nav = null
	Navigation.headerHotspot = null
	Navigation.footerHotspot = null
	Navigation.previousButton = null
	Navigation.nextButton = null
	Navigation.firstButton = null
	Navigation.lastButton = null
	Navigation.topButton = null
	Navigation.bottomButton = null
	Navigation.search = null
	Navigation.searchResult = null
	Navigation.gotoPage = null

	Navigation.init = function(search, searchResult, gotoPage)
	{
		var that = this

		this.input = document.getElementById("page-index-input")
		this.previousButton = document.getElementById("nav-button-previous")
		this.nextButton = document.getElementById("nav-button-next")
		this.firstButton = document.getElementById("nav-button-first")
		this.lastButton = document.getElementById("nav-button-last")
		this.topButton = document.getElementById("nav-button-top")
		this.bottomButton = document.getElementById("nav-button-bottom")
		this.nav = document.getElementById("header")
		this.headerHotspot = document.getElementById("header-hotspot")
		this.footerHotspot = document.getElementById("footer-hotspot")
		this.search = search
		this.searchResult = searchResult
		this.gotoPage = gotoPage
		this.previousButton.addEventListener("click", function(event) { that.gotoPreviousPage() })
		this.nextButton.addEventListener("click", function(event) { that.gotoNextPage() })
		this.firstButton.addEventListener("click", function(event) { that.gotoFirstPage() })
		this.lastButton.addEventListener("click", function(event) { that.gotoLastPage() })
		this.topButton.addEventListener("click", function(event) { that.scrollToTop() })
		this.bottomButton.addEventListener("click", function(event) { that.scrollToBottom() })
		this.headerHotspot.addEventListener("mouseover", function(event) { that.setAnchor("top") })
		this.footerHotspot.addEventListener("mouseover", function(event) { that.setAnchor("bottom") })
		this.nav.addEventListener("mouseleave", function(event) { if (that.input.hasFocus()) { that.input.blur() } })

		this.hookPageIndexInput()
		this.validate()
		this.hookScroll()
		this.validateScroll()
	}

	Navigation.scrollToTop = function()
	{
		self.scrollTo(0, 0)
	}

	Navigation.scrollToBottom = function()
	{
		self.scrollTo(0, document.body.scrollHeight)
	}

	Navigation.validate = function()
	{
		if (this.search.page_index <= 1)
		{
			this.firstButton.disabled = true
			this.previousButton.disabled = true
		}

		if (this.search.page_index >= this.searchResult.num_pages)
		{
			this.nextButton.disabled = true
			this.lastButton.disabled = true
		}
	}

	Navigation.validateScroll = function()
	{
		var y = document.documentElement.scrollTop
		var h = document.documentElement.scrollHeight - document.documentElement.clientHeight

		this.topButton.disabled = (y <= 0)
		this.bottomButton.disabled = (y >= h)
	}

	Navigation.setAnchor = function(anchor)
	{
		var classNameToRemove
		var classNameToAdd

		if (anchor === "top")
		{
			classNameToRemove = "fixed-bottom"
			classNameToAdd = "fixed-top"
		}
		else if (anchor === "bottom")
		{
			classNameToRemove = "fixed-top"
			classNameToAdd = "fixed-bottom"
		}
		else
		{
			return
		}

		this.nav.removeClass(classNameToRemove)
		this.nav.addClass(classNameToAdd)
	}

	Navigation.gotoPreviousPage = function()
	{
		if (this.search.page_index <= 1)
		{
			return
		}

		this.gotoPage(this.search.page_index - 1)
	}

	Navigation.gotoNextPage = function()
	{
		if (this.search.page_index >= this.searchResult.num_pages)
		{
			return
		}

		this.gotoPage(this.search.page_index + 1)
	}

	Navigation.gotoFirstPage = function()
	{
		if (this.search.page_index <= 1)
		{
			return
		}

		this.gotoPage(1)
	}

	Navigation.gotoLastPage = function()
	{
		if (this.search.page_index >= this.searchResult.num_pages)
		{
			return
		}

		this.gotoPage(this.searchResult.num_pages)
	}

	Navigation.hookScroll = function()
	{
		var that = this

		document.addEventListener("scroll", function()
		{
			that.validateScroll()
		})
	}

	Navigation.hookPageIndexInput = function()
	{
		var that = this

		this.input.addEventListener("keydown", function(event)
		{
			event = event || window.event
			var keyCode = event.keyCode || event.which

			if (keyCode === 13)
			{
				if (!that.submitPageIndex())
				{
					return true
				}

				return event.markAsHandled()
			}
		})
	}

	Navigation.submitPageIndex = function()
	{
		var val = parseInt(this.input.value)

		if (isNaN(val)
			|| val < 0
			|| val > this.searchResult.num_pages - 1
			/*|| val === this.search.page_index*/)  // NOTE: allow reload of the same page
		{
			return false
		}

		this.gotoPage(val)

		return true
	}
}
