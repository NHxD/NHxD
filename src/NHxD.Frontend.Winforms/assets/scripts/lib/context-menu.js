{
	var ContextMenus = {}

	ContextMenus.contextMenus = []
	ContextMenus.delayedShowTimer = null

	ContextMenus.clear = function()
	{
		this.contextMenus = []
	}

	ContextMenus.register = function(contextMenu)
	{
		this.contextMenus.push(contextMenu)
	}

	ContextMenus.closeAll = function()
	{
		for (var i = 0; i < this.contextMenus.length; ++i)
		{
			var contextMenu = this.contextMenus[i]

			if (contextMenu)
			{
				contextMenu.close()
			}
		}
	}

	ContextMenus.show = function(contextMenu, target)
	{
		var hadAnyMenuOpened = false

		for (var i = 0; i < this.contextMenus.length; ++i)
		{
			var contextMenu2 = this.contextMenus[i]

			if (contextMenu2.isClosed())
			{
				continue
			}

			contextMenu2.close()
			hadAnyMenuOpened = true
		}

		// force a delay because menus can sometime be opened at the same location
		// (e.g., often the case when they're near the edge of the viewport)
		// and it can be very confusing when that happens
		if (hadAnyMenuOpened)
		{
			if (this.delayedShowTimer)
			{
				window.clearTimeout(this.delayedShowTimer)
			}
			this.delayedShowTimer = window.setTimeout(function()
			{
				contextMenu.show(target)
			}, 100)
		}
		else
		{
			contextMenu.show(target)
		}
	}

	ContextMenus.init = function(props)
	{
		var propNames = [
			"contextMenus"
		]

		for (var i = 0; i < propNames.length; ++i)
		{
			var propName = propNames[i]

			if (typeof props[propName] !== "undefined")
			{
				this[propName] = props[propName]
			}
		}

		document.body.addEventListener("mouseleave", function(event)
		{
			ContextMenus.closeAll()
		})

		document.body.addEventListener("click", function(event)
		{
			var event = event || window.event

			if (event.target)
			{
				var isMenu = event.target.tagName === "DIV" && event.target.hasClass("context-menu")
				//var isCommand = event.target.tagName === "BUTTON" && event.target.hasClass("menu-command")
				var isSeparator = event.target.tagName === "HR"
				var isDisabled = event.target.disabled

				if (isMenu
					//|| isCommand
					|| isSeparator
					|| isDisabled)
				{
					return event.markAsHandled()
				}
			}

			ContextMenus.closeAll()
		})
	}
}

{
	//
	// requires html: element with id "context-menu-xxx" (where xxx is the name of the menu)
	// requires css: .display-none
	//

	var ContextMenu = {}

	ContextMenu.name = ""
	ContextMenu.target = null
	ContextMenu.element = null
	ContextMenu.horizontalAlignment = "left"
	ContextMenu.verticalAlignment = "bottom"
	ContextMenu.horizontalSide = "inside"
	ContextMenu.verticalSide = "outside"
	ContextMenu.onbeforeopen = null
	ContextMenu.onopen = null

	ContextMenu.exists = function()
	{
		return this.element !== null
	}

	ContextMenu.show = function(target)
	{
		if (this.onbeforeopen)
		{
			if (this.onbeforeopen(target))
			{
				return
			}
		}

		this.fixSeparators()

		var top = 0
		var left = 0
		var element = target

		if (this.horizontalAlignment === "cursor")
		{
			left = this.lastClientX
		}
		else if (this.horizontalAlignment === "left" && this.horizontalSide === "inside")
		{
			left = 0
		}
		else if (this.horizontalAlignment === "left" && this.horizontalSide === "outside")
		{
			left = -this.element.offsetWidth
		}
		else if (this.horizontalAlignment === "right" && this.horizontalSide === "inside")
		{
			left = element.offsetLeft - this.element.offsetWidth
		}
		else if (this.horizontalAlignment === "right" && this.horizontalSide === "outside")
		{
			left = element.offsetLeft
		}
		else if (this.horizontalAlignment === "center")
		{
			left = element.offsetLeft + (element.offsetWidth / 2) - this.element.offsetWidth
		}

		if (this.verticalAlignment === "cursor")
		{
			top = this.lastClientY
		}
		else if (this.verticalAlignment === "top" && this.verticalSide === "inside")
		{
			top = 0
		}
		else if (this.verticalAlignment === "top" && this.verticalSide === "outside")
		{
			top = -this.element.offsetHeight
		}
		else if (this.verticalAlignment === "bottom" && this.verticalSide === "inside")
		{
			top = element.offsetHeight - this.element.offsetHeight
		}
		else if (this.verticalAlignment === "bottom" && this.verticalSide === "outside")
		{
			top = element.offsetHeight
		}
		else if (this.verticalAlignment === "middle")
		{
			top = element.offsetHeight - (element.offsetTop / 2)
		}
		
		if (this.horizontalAlignment === "cursor")
		{
			left += document.body.offsetLeft + document.documentElement.scrollLeft
		}
		else
		{
			var tempElement = element

			do {
				if (tempElement.offsetLeft)
				{
					left += tempElement.offsetLeft
				}

				tempElement = tempElement.offsetParent
			} while (tempElement)
		}

		if (this.verticalAlignment === "cursor")
		{
			top += document.body.offsetTop + document.documentElement.scrollTop
		}
		else
		{
			var tempElement = element

			do {
				if (tempElement.offsetTop)
				{
					top += tempElement.offsetTop
				}

				tempElement = tempElement.offsetParent
			} while (tempElement)
		}

		var dims = this.getComputedDimensions()

		if (!dims.visibleChildCount)
		{
			return
		}

		if (this.element.style.zoom)
		{
			dims.width *= this.element.style.zoom
			dims.height *= this.element.style.zoom
		}

		//
		// FIXME: the context menu extents past the client area (making the scroll bar appear)
		//

		if (top < document.documentElement.scrollTop)
		{
			top = document.documentElement.scrollTop
		}
		else if (top + dims.height > document.documentElement.scrollTop + document.documentElement.clientHeight)
		{
			top = document.documentElement.scrollTop + document.documentElement.clientHeight - dims.height
		}

		if (left < document.documentElement.scrollLeft)
		{
			left = document.documentElement.scrollLeft
		}
		else if (left + dims.width > document.documentElement.scrollLeft + document.documentElement.clientWidth)
		{
			left = document.documentElement.scrollLeft + document.documentElement.clientWidth - dims.width
		}

		this.element.style.top = top + "px"
		this.element.style.left = left + "px"
		this.element.removeClass("display-none")
		this.target = target

		if (this.onopen)
		{
			this.onopen(target)
		}
	}

	ContextMenu.fixSeparators = function()
	{
		var children = this.element.getElementsByTagName("HR")

		for (var i = 0; i < children.length; ++i)
		{
			var child = children[i]
			var previousVisibleSibling = this.findVisibleSibling(child, function(x) { return x.previousElementSibling })
			var nextVisibleSibling = this.findVisibleSibling(child, function(x) { return x.nextElementSibling })

			child.addOrRemoveClass("display-none",
				(previousVisibleSibling === null || nextVisibleSibling === null)
				|| (i > 0 && previousVisibleSibling.getSiblingIndex() === children[i - 1].getSiblingIndex()))
		}
	}

	ContextMenu.findVisibleSibling = function(element, fieldSelector)
	{
		if (element)
		{
			while (element = fieldSelector(element))
			{
				if (element !== null
					&& !element.hasClass("display-none"))
				{
					return element
				}
			}
		}

		return null
	}

	ContextMenu.getComputedDimensions = function()
	{
		var children = this.element.getElementsByTagName("BUTTON")
		var visibleChildCount = 0
		var itemHeight = 0

		for (var i = 0; i < children.length; ++i)
		{
			var child = children[i]

			if (!child.hasClass("menu-command"))
			{
				continue
			}

			if (child.hasClass("display-none"))
			{
				continue
			}

			itemHeight = Math.max(itemHeight, parseInt(child.style.height))

			++visibleChildCount
		}

		children = this.element.getElementsByTagName("HR")
		var visibleSeparators = 0

		for (var i = 0; i < children.length; ++i)
		{
			var child = children[i]

			if (child.hasClass("display-none"))
			{
				continue
			}

			++visibleSeparators
		}

		var itemWidth = parseInt(this.element.style.width)
		var itemSpacing = parseInt(this.element.style.paddingBottom)

		return {
			visibleChildCount: visibleChildCount,
			visibleSeparatorCount: visibleSeparators,
			width: itemWidth,
			height: (itemHeight * visibleChildCount) + (itemSpacing * visibleSeparators)
		}
	}

	ContextMenu.close = function()
	{
		this.element.addClass("display-none")
	}

	ContextMenu.isClosed = function()
	{
		return this.element.hasClass("display-none")
	}

	ContextMenu.create = function(items)
	{
		if (this.exists())
		{
			return
		}

		var div = document.createElement("DIV")

		document.body.appendChild(div)

		div.id = "context-menu-" + this.name
		this.element = div

		// TODO: add items
	}

	ContextMenu.init = function(props)
	{
		var propNames =
		[
			"name",
			"onbeforeopen",
			"onopen",
			"target",
			"horizontalAlignment",
			"verticalAlignment",
			"horizontalSide",
			"verticalSide"
		]

		for (var i = 0; i < propNames.length; ++i)
		{
			var propName = propNames[i]

			if (typeof props[propName] !== "undefined")
			{
				this[propName] = props[propName]
			}
		}

		if (!this.name)
		{
			return
		}

		this.element = document.getElementById("context-menu-" + this.name)

		var that = this

		document.addEventListener("mousedown",
			function(event)
			{
				event = event || window.event

				that.lastClientX = event.clientX// + document.body.scrollLeft + document.documentElement.scrollLeft
				that.lastClientY = event.clientY// + document.body.scrollTop + document.documentElement.scrollTop
			})
	}

	ContextMenu.remove = function()
	{
		if (!this.exists())
		{
			return
		}

		document.body.removeChild(this.element)
	}
}
