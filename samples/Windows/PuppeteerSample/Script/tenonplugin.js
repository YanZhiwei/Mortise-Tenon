// import { elementFromPointResult } from "./result/elementFromPointResult";

class elementFromPointResult {
  constructor(customId, iframeIds) {
    this.customId = customId;
    this.iframeIds = iframeIds;
  }
}

window.tenon_pluginsMap = {
  pingTab: function (param) {
    /*      console.log("Plugin called with parameter: " + param)*/
    return "Plugin called with parameter: " + param;
  },
  elementFromPoint: function (point) {
    // 边界情况处理
    if (
      !point ||
      typeof point !== "object" ||
      !("x" in point) ||
      !("y" in point)
    ) {
      throw new Error("Invalid point object");
    }

    let { x: offsetX, y: offsetY } = point;
    let foundElem = null,
      lastFrame = null;
    let totalFrameOffset = { x: 0, y: 0 };
    let doc = window.document;

    while (doc) {
      const xOfsInDoc = offsetX - totalFrameOffset.x;
      const yOfsInDoc = offsetY - totalFrameOffset.y;

      foundElem = doc.elementFromPoint(xOfsInDoc, yOfsInDoc);

      if (foundElem == null) {
        foundElem = lastFrame;
        break;
      }
      const foundElemTagName = foundElem.tagName.toLowerCase();
      if (foundElemTagName === "frame" || foundElemTagName === "iframe") {
        lastFrame = foundElem;
        const frameRect = lastFrame.getBoundingClientRect();
        totalFrameOffset.x += frameRect.left;
        totalFrameOffset.y += frameRect.top;
        doc = lastFrame.contentDocument || lastFrame.contentWindow.document;
      } else {
        break;
      }
    }

    if (foundElem == null) {
      return -1;
    }
    if (foundElem && lastFrame) {
      const elemRect = foundElem.getBoundingClientRect();
      const frameRect = lastFrame.getBoundingClientRect();

      // 性能优化：减少重复计算
      const frameWidthLessThanElem = frameRect.width < elemRect.width;
      const frameHeightLessThanElem = frameRect.height < elemRect.height;

      if (frameWidthLessThanElem || frameHeightLessThanElem)
        foundElem = lastFrame;
    }
    let customId = window.tenon_pluginsMap.setCustomId(foundElem);
    var iframestack = [];
    iframestack.push(1);
    return new elementFromPointResult(customId, iframestack);
  },
  getElementRect: function (customId) {
    const elem = window.document.querySelector('[customid="' + customId + '"]');
    if (!elem) {
      console.log(`Element:${customId} not found`);
      return null;
    }
    const rect = elem.getBoundingClientRect();
    const finalRect = {
      left: Math.round(rect.left),
      top: Math.round(rect.top),
      width: Math.round(rect.width),
      height: Math.round(rect.height),
    };
    window.tenon_pluginsMap.drawRect(finalRect);
    return finalRect;
  },
  setCustomId: function (elem) {
    let customId = elem.getAttribute("customid");
    if (customId) {
      return customId;
    }
    customId = Math.random().toString(16).slice(2);
    elem.setAttribute("customid", customId);
    return customId;
  },
  getPageRect: function () {
    const rect = document.body.getBoundingClientRect();
    return {
      left: rect.left,
      top: rect.top,
      width: rect.width,
      height: rect.height,
    };
  },
  getPageScroll: function () {
    return {
      scrollX: window.scrollX,
      scrollY: window.scrollY,
    };
  },
  getPageZoom: function () {
    return {
      zoom: window.devicePixelRatio,
    };
  },
  drawRect: function (rect) {
    const { left, top, width, height } = rect;
    const rectDiv = document.createElement("div");
    rectDiv.style.position = "fixed";
    rectDiv.style.left = left + "px";
    rectDiv.style.top = top + "px";
    rectDiv.style.width = width + "px";
    rectDiv.style.height = height + "px";
    rectDiv.style.border = "2px solid red";
    rectDiv.style.zIndex = 9999;
    document.body.appendChild(rectDiv);
    setTimeout(() => {
      document.body.removeChild(rectDiv);
    }, 5000);
  },
};

window.tenon_call_plugin = async function (pluginName, param) {
  try {
    return {
      code: 1,
      result: await (async function (pluginName, param) {
        const plugin = window.tenon_pluginsMap[pluginName];
        if (!plugin) {
          throw new Error("Plugin: " + pluginName + " isn't registered.");
        }
        return plugin(param);
      })(pluginName, param),
    };
  } catch (error) {
    return {
      code: error.code || 500,
      message: `Error calling ${pluginName}: ${error.message}; stack: ${
        error.stack || ""
      }`,
    };
  }
};
