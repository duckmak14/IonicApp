
CKEDITOR.editorConfig = function (config) {
    config.language = 'vi';
    // config.uiColor = '#AADC6E';
    config.language = 'vi';
    config.filebrowserBrowseUrl = '/content/ckfinder/news.html';
    config.filebrowserImageBrowseUrl = '/content/ckfinder/common.html?Type=Images';
    config.filebrowserFlashBrowseUrl = '/content/ckfinder/common.html?Type=Flash';
    config.filebrowserUploadUrl = '/content/ckfinder/core/connector/aspx/common.aspx?command=QuickUpload&type=Files';
    config.filebrowserImageUploadUrl = '/admin/upload/CkImageUpload/1'; 
    config.filebrowserFlashUploadUrl = '/content/ckfinder/core/connector/aspx/common.aspx?command=QuickUpload&type=Flash';
    config.toolbar = [
        { name: 'document', groups: ['mode', 'document', 'doctools'], items: ['Source', '-', 'Save', 'NewPage', 'Preview', 'Print', '-', 'Templates'] },
        { name: 'clipboard', groups: ['clipboard', 'undo'], items: ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'RemoveFormat', '-', 'Undo', 'Redo'] },
        { name: 'forms', items: ['Form', 'Checkbox', 'Radio', 'TextField', 'Textarea', 'Select', 'Button', 'ImageButton', 'HiddenField'] },
        '/',
        { name: 'editing', groups: ['find', 'selection', 'spellchecker'], items: ['Find', 'Replace', '-', 'SelectAll', '-', 'Scayt'] },
        { name: 'basicstyles', groups: ['basicstyles', 'cleanup'], items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript'] },
        { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align', 'bidi'], items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', 'CreateDiv', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', '-', 'BidiLtr', 'BidiRtl'] },
        { name: 'links', items: ['Link', 'Unlink', 'Anchor'] },
         '/',
        { name: 'insert', items: ['Image', 'Flash', 'Syntaxhighlight', 'Table', 'HorizontalRule', 'Smiley', 'SpecialChar', 'PageBreak', 'Iframe'] },
        { name: 'styles', items: ['Styles', 'Format', 'Font', 'FontSize'] },
        { name: 'colors', items: ['TextColor', 'BGColor'] },
        { name: 'tools', items: ['Maximize', 'ShowBlocks'] },
        { name: 'others', items: ['-'] },
        ///{ name: 'about', items: ['About'] }
    ];

    config.toolbarGroups = [
        { name: 'document', groups: ['mode', 'document', 'doctools'] },
        { name: 'clipboard', groups: ['clipboard', 'undo'] },
        { name: 'editing', groups: ['find', 'selection', 'spellchecker'] },
        { name: 'forms' },
        '/',
        { name: 'basicstyles', groups: ['basicstyles', 'cleanup'] },
        { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align', 'bidi'] },
        { name: 'links' },
        { name: 'insert' },
        '/',
        { name: 'styles' },
        { name: 'colors' },
        { name: 'tools' },
        { name: 'others' },
        { name: 'about' }
    ];
};

CKFinder.setupCKEditor(null, '/content/ckfinder/');