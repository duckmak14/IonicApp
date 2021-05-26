kendo.ui.Locale = "VietNam (vi-VN)";
kendo.ui.ColumnMenu.prototype.options.messages = 
  $.extend(kendo.ui.ColumnMenu.prototype.options.messages, {

/* COLUMN MENU MESSAGES 
 ****************************************************************************/   
  sortAscending: "Sắp xếp tăng dần",
  sortDescending: "Sắp xếp giảm dần",
  filter: "Filter",
  columns: "Columns"
 /***************************************************************************/   
});

kendo.ui.Groupable.prototype.options.messages = 
  $.extend(kendo.ui.Groupable.prototype.options.messages, {

/* GRID GROUP PANEL MESSAGES 
 ****************************************************************************/   
  empty: "Kéo cột lựa chọn vào đây để nhóm"
 /***************************************************************************/   
});

kendo.ui.FilterMenu.prototype.options.messages = 
  $.extend(kendo.ui.FilterMenu.prototype.options.messages, {
  
/* FILTER MENU MESSAGES 
 ***************************************************************************/   
  info: "", // sets the text on top of the filter menu
  isTrue: "is true",                   // sets the text for "isTrue" radio button
  isFalse: "is false",                 // sets the text for "isFalse" radio button
  filter: "Lọc",                    // sets the text for the "Filter" button
  clear: "Xóa lọc",                      // sets the text for the "Clear" button
  and: "Và",
  or: "Hoặc",
  selectValue: "-Select value-"
 /***************************************************************************/   
});
         
kendo.ui.FilterMenu.prototype.options.operators =           
  $.extend(kendo.ui.FilterMenu.prototype.options.operators, {

/* FILTER MENU OPERATORS (for each supported data type) 
 ****************************************************************************/   
  string: {
      eq: "Bằng",                 //Is equal to
      neq: "Không bằng",          //Is not equal to
      startswith: "Bắt đầu bằng",  //  Starts with
      contains: "Chứa",             //Contains
      doesnotcontain: "Không chứa",   //Does not contain
      endswith: "Kết thúc bằng"         //Ends with
  },
  number: {
      eq: "Bằng",
      neq: "Không bằng",
      gte: "Lớn hơn hoặc bằng",
      gt: "Lớn hơn",
      lte: "Nhỏ hơn hoặc bằng",
      lt: "Nhỏ hơn"
  },
  date: {
      eq: "Bằng",
      neq: "Không bằng",
      gte: "Sau hoặc bằng",
      gt: "Sau",
      lte: "Trước hoặc bằng",
      lt: "Trước"
  },
  enums: {
      eq: "Bằng",
      neq: "Không bằng"
  }
 /***************************************************************************/   
});

kendo.ui.Pager.prototype.options.messages = 
  $.extend(kendo.ui.Pager.prototype.options.messages, {
  
/* PAGER MESSAGES 
 ****************************************************************************/   
  display: "{0} - {1} of {2} items",
  empty: "No items to display",
  page: "Page",
  of: "of {0}",
  itemsPerPage: "items per page",
  first: "Go to the first page",
  previous: "Go to the previous page",
  next: "Go to the next page",
  last: "Go to the last page",
  refresh: "Refresh"
 /***************************************************************************/   
});

kendo.ui.Validator.prototype.options.messages = 
  $.extend(kendo.ui.Validator.prototype.options.messages, {

/* VALIDATOR MESSAGES 
 ****************************************************************************/   
  required: "{0} is required",
  pattern: "{0} is not valid",
  min: "{0} should be greater than or equal to {1}",
  max: "{0} should be smaller than or equal to {1}",
  step: "{0} is not valid",
  email: "{0} is not valid email",
  url: "{0} is not valid URL",
  date: "{0} is not valid date"
 /***************************************************************************/   
});

kendo.ui.ImageBrowser.prototype.options.messages = 
  $.extend(kendo.ui.ImageBrowser.prototype.options.messages, {

/* IMAGE BROWSER MESSAGES 
 ****************************************************************************/   
  uploadFile: "Upload",
  orderBy: "Arrange by",
  orderByName: "Name",
  orderBySize: "Size",
  directoryNotFound: "A directory with this name was not found.",
  emptyFolder: "Empty Folder",
  deleteFile: 'Are you sure you want to delete "{0}"?',
  invalidFileType: "The selected file \"{0}\" is not valid. Supported file types are {1}.",
  overwriteFile: "A file with name \"{0}\" already exists in the current directory. Do you want to overwrite it?",
  dropFilesHere: "drop files here to upload"
 /***************************************************************************/   
});

kendo.ui.Editor.prototype.options.messages = 
  $.extend(kendo.ui.Editor.prototype.options.messages, {

/* EDITOR MESSAGES 
 ****************************************************************************/   
  bold: "Bold",
  italic: "Italic",
  underline: "Underline",
  strikethrough: "Strikethrough",
  superscript: "Superscript",
  subscript: "Subscript",
  justifyCenter: "Center text",
  justifyLeft: "Align text left",
  justifyRight: "Align text right",
  justifyFull: "Justify",
  insertUnorderedList: "Insert unordered list",
  insertOrderedList: "Insert ordered list",
  indent: "Indent",
  outdent: "Outdent",
  createLink: "Insert hyperlink",
  unlink: "Remove hyperlink",
  insertImage: "Insert image",
  insertHtml: "Insert HTML",
  fontName: "Select font family",
  fontNameInherit: "(inherited font)",
  fontSize: "Select font size",
  fontSizeInherit: "(inherited size)",
  formatBlock: "Format",
  foreColor: "Color",
  backColor: "Background color",
  style: "Styles",
  emptyFolder: "Empty Folder",
  uploadFile: "Upload",
  orderBy: "Arrange by:",
  orderBySize: "Size",
  orderByName: "Name",
  invalidFileType: "The selected file \"{0}\" is not valid. Supported file types are {1}.",
  deleteFile: 'Are you sure you want to delete "{0}"?',
  overwriteFile: 'A file with name "{0}" already exists in the current directory. Do you want to overwrite it?',
  directoryNotFound: "A directory with this name was not found.",
  imageWebAddress: "Web address",
  imageAltText: "Alternate text",
  dialogInsert: "Insert",
  dialogButtonSeparator: "or",
  dialogCancel: "Cancel"
 /***************************************************************************/   
});
