$(function(){
    $('.button-add').click(function(event){
        $('.add-cars-form').addClass('active');
    });
    $('.exit').click(function(event){
        $('.add-cars-form').removeClass('active');
    });
    $('.add-accept').click(function(event){
        $('.add-cars-form').removeClass('active');
        alert('Đã thêm xe');
    });
    $('.open-menu').click(function(event){
        $('.menu').addClass('show-menu');
    });
    $('.content').click(function(event){
        $('.menu').removeClass('show-menu');
    })
    $('.btn-change').click(function() {
        $('.modal-bill').addClass('active-modal-bill');
    });
    $('.close-modal').click(function(event) {
        $('.modal-bill').removeClass('active-modal-bill');
    });

});  // Js date time picker
            // Js checklist

$(function(){
    $('.checked_all').on('change', function() {
        $('.checkbox').prop('checked', $(this).prop("checked"));
    });

    $('.checkbox').change(function() {
        if ($('.checkbox:checked').length == $('.checkbox').length) {
            $('.checked_all').prop('checked', true);
        } else {
            $('.checked_all').prop('checked', false);
        }
    });
});

    // Js date picker
$(function() {
    $('input[name="date"]').daterangepicker({
        singleDatePicker: true,
        showDropdowns: true,
        minYear: 1991,
        maxYear: parseInt(moment().format('YYYY'), 10)
    }, function(start, end, label) {
        var years = moment().diff(start, 'years');
    });
});