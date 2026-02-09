/**
 * تحويل base64 string إلى File object
 * @param {string} base64String - البيانات بصيغة base64 (مع أو بدون data URL prefix)
 * @param {string} fileName - اسم الملف
 * @returns {File} - كائن File
 */
export const convertBase64ToFile = (base64String, fileName) => {
    // إزالة data URL prefix إذا كان موجوداً (data:image/jpeg;base64,)
    const base64Data = base64String.includes(',') 
        ? base64String.split(',')[1] 
        : base64String;
    
    // تحويل base64 إلى binary
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: 'image/jpeg' });
    
    // إنشاء File object
    return new File([blob], fileName, { type: 'image/jpeg' });
};

/**
 * دالة المسح الضوئي
 * @param {Function} onSuccess - callback عند نجاح المسح (يستقبل مصفوفة من File objects)
 * @param {Function} onError - callback عند حدوث خطأ (يستقبل رسالة الخطأ)
 */
export const handleScan = (onSuccess, onError) => {
    // التحقق من وجود window.scanner
    if (!window.scanner) {
        onError('مكتبة المسح الضوئي غير متوفرة. يرجى التأكد من تحميل السكريبت.');
        return;
    }

    // استدعاء scanner.scan
    window.scanner.scan(
        (successful, mesg, response) => {
            if (!successful) {
                // في حالة الخطأ
                onError('فشل المسح الضوئي: ' + mesg);
                return;
            }

            // التحقق من إلغاء المستخدم
            if (successful && mesg != null && mesg.toLowerCase().indexOf('user cancel') >= 0) {
                onError('تم إلغاء المسح الضوئي');
                return;
            }

            try {
                // الحصول على الصور الممسوحة
                const scannedImages = window.scanner.getScannedImages(response, true, false);
                
                if (!scannedImages || !Array.isArray(scannedImages) || scannedImages.length === 0) {
                    onError('لم يتم العثور على صور ممسوحة');
                    return;
                }

                // تحويل الصور من base64 إلى File objects
                const now = new Date();
                const dateStr = now.toISOString().slice(0, 10).replace(/-/g, '-');
                const timeStr = now.toTimeString().slice(0, 8).replace(/:/g, '');
                
                const fileObjects = scannedImages.map((scannedImage, index) => {
                    // إنشاء اسم ملف بصيغة: مسح-ضوئي-YYYY-MM-DD-HHmmss-1.jpg
                    const fileName = `مسح-ضوئي-${dateStr}-${timeStr}-${index + 1}.jpg`;
                    
                    // تحويل base64 إلى File
                    return convertBase64ToFile(scannedImage.src, fileName);
                });

                // استدعاء callback النجاح مع ملفات File
                onSuccess(fileObjects);
            } catch (error) {
                console.error('خطأ في معالجة الصور الممسوحة:', error);
                onError('حدث خطأ أثناء معالجة الصور الممسوحة: ' + error.message);
            }
        },
        {
            use_asprise_dialog: true, // استخدام نافذة Asprise
            show_scanner_ui: true, // إظهار واجهة المسح الضوئي
            twain_cap_setting: {
                ICAP_PIXELTYPE: "TWPT_RGB" // ألوان
            },
            output_settings: [{
                type: "return-base64",
                format: "jpg"
            }]
        }
    );
};

