CKEDITOR.plugins.add('abbr',
{
    init: function (editor) {
        editor.addCommand('abbrDialog', new CKEDITOR.dialogCommand('abbrDialog'));
        editor.ui.addButton('Abbr',
		{
		    label: 'Insert Abbreviation',
		    command: 'abbrDialog',
		    icon: this.path + 'images/icon.png'
		});
        CKEDITOR.dialog.add('abbrDialog', function (editor) {
            return {
                title: 'Abbreviation Properties',
                minWidth: 400,
                minHeight: 200,
                contents:
				[
					{
					    id: 'tab1',
					    label: 'Basic Settings',
					    elements:
						[
							{
							    type: 'text',
							    id: 'abbr',
							    label: 'Abbreviation',
							    validate: CKEDITOR.dialog.validate.notEmpty("Abbreviation field cannot be empty")
							},
							{
							    type: 'text',
							    id: 'title',
							    label: 'Explanation',
							    validate: CKEDITOR.dialog.validate.notEmpty("Explanation field cannot be empty")
							}
						]
					},
					{
					    id: 'tab2',
					    label: 'Advanced Settings',
					    elements:
						[
							{
							    type: 'text',
							    id: 'id',
							    label: 'Id'
							}
						]
					}
				],
                onOk: function () {
                    var dialog = this;

                    var abbr = editor.document.createElement('abbr');
                    abbr.setAttribute('title', dialog.getValueOf('tab1', 'title'));
                    abbr.setText(dialog.getValueOf('tab1', 'abbr'));
                    var id = dialog.getValueOf('tab2', 'id');
                    if (id)
                        abbr.setAttribute('id', id);
                    editor.insertElement(abbr);
                }
            };
        });
    }
});