$(function () {


});
//Show the accordion which was last shown for layout areas.
$(window).on('load', function () {
});


export function setupEditWebpage(){
    
    $.validator.setDefaults({ ignore: "" }); // validate hidden tabs
    
    const scope = $('[data-edit-webpage]');

    scope.find("#UrlSegment").blur(function (e) {
        const that = $(this);
        that.val(that.val().trim().replace(/[^a-zA-Z0-9-/]/g, '-'));
    });

    // scope.find('#publish-status-change').click(function (e) {
    //     e.preventDefault();
    //     const form = scope;
    //     if (scope.find("#PublishOn").val()?.length > 0) {
    //         scope.find("#PublishOn").val('');
    //         form.submit();
    //     } else {
    //         $.get('/Admin/Webpage/GetServerDate', function (response) {
    //             scope.find("#PublishOn").val(response);
    //             form.submit();
    //         });
    //     }
    //     return false;
    // });

    //Show box for chaging the URL which is hidden by default.
    scope.find("#change-url").click(function (e) {
        e.preventDefault();
        scope.find("#UrlSegment").removeAttr('disabled')
    });

    scope.find('#accordion-layout-areas').on('shown', function (e) {
        store.set('selected-layout-area-' + location.pathname, e.target.id);
    });

    scope.find('#accordion-layout-areas').on('hidden', function (e) {
        store. set('selected-layout-area-' + location.pathname, '');
    });

    scope.find('#PublishOn').change(function () {
        scope.find('#publish-on-hidden').val($(this).val());
    });

    // lazy load iframe preview when tab is selected
    // needs to be before shown from hash/cookie
    scope.find('.main-content a[data-toggle="tab"]').on('shown', function (e) {
        const tab = e.target.toString();
        if (tab.length > 0) {
            if (tab.indexOf("preview") !== -1) {
                scope.find("#previewIframe").attr('src', '/' + scope.find('#LiveUrlSegment').val());
            }
        }
    });

    //Permissions
    $.get('/Admin/Role/GetRolesForPermissions', function (data) {
        scope.find("#FrontEndRoles").tagit({
            autocomplete: { delay: 0, minLength: 0, source: data },
            availableTags: data,
            showAutocompleteOnFocus: true,
            singleField: true,
            placeholderText: "Click to add role."
        });
    });

    scope.find("#MetaKeywords").tagit({
        tagLimit: 15,
        allowSpaces: true
    });

    scope.on('change','#HasCustomPermissions',setPermissionVisibility);
    scope.on('change','#PermissionType',setPermissionVisibility);


    function setPermissionVisibility() {
        const form = scope;

        const hasCustomPermissions = form.find('#HasCustomPermissions');
        const customPermissions = form.find('[data-custom-permissions]');
        const permissionType = customPermissions.find('#PermissionType');
        const permissionTypeHolders = customPermissions.find('[data-custom-permissions-type]');
        permissionTypeHolders.hide();

        const showCustomPermissions = hasCustomPermissions.is(':checked');
        customPermissions.toggle(showCustomPermissions);

        if (!showCustomPermissions) {
            return;
        }

        const permissionTypeValue = permissionType.val();
        customPermissions.find('[data-custom-permissions-type*="' + permissionTypeValue + '"]').show();
    }

    setPermissionVisibility(); 
    
    $(document).on('keypress',
        function (event) {
            if (event.which === 13) {
                if (scope.find('#SEOTargetPhrase:focus').length) {
                    event.preventDefault();
                    scope.find('[data-seo-analyze]').click();
                }
                if (scope.find('input#Search:focus').length) {
                    event.preventDefault();
                    scope.find('#search-postings').click();
                }
            }

        });
    $(document).on('click', '[data-seo-analyze]', function (event) {
        event.preventDefault();
        const value = scope.find('#SEOTargetPhrase').val();
        if (value === '') {
            scope.find('[data-seo-analysis-summary]').html('Please enter a term to analyze.');
        } else {
            scope.find('[data-seo-analysis-summary]').html('Analyzing...');
            $.get('/Admin/SEOAnalysis/Analyze', { id: scope.find('#Id').val(), SEOTargetPhrase: value }, function (response) {
                scope.find('[data-seo-analysis-summary]').html(response);
            });
        }
    });
    if (store.get('selected-layout-area-' + location.pathname)) {
        scope.find('#accordion-layout-areas a[href="#' + store.get('selected-layout-area-' + location.pathname) + '"]').click();
    }
}