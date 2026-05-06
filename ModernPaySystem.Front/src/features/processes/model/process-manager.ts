import { useState, useRef } from 'react';
import { useQueryState, parseAsInteger, parseAsString } from 'nuqs';
import type { Client, KinshipType, Service, OperationDetail, Operation, ProcessFormData } from '@/entities/processes/types/process-types';

// Custom hook for managing processes
export const useProcessManager = () => {
    // ... rest of state stays same ...
    const [formData, setFormData] = useState<ProcessFormData>({
        CODE: 0,
        CODE_CLIENT: 0,
        CODE_CLIENT2: 0,
        CODE_KINSHIP: 0,
        SUM_AMOUNT: 0,
        DATE_ACTION: new Date().toISOString().split('T')[0],
        NOTES: '',
        user_no: 1,
        Date_Of_work: new Date().toISOString().split('T')[0]
    });

    const [clients] = useState<Client[]>([
        { Client_No: 1, FIRST_NAME: 'أحمد', FATHER_NAME: 'محمد', LAST_NAME: 'علي' },
        { Client_No: 2, FIRST_NAME: 'سارة', FATHER_NAME: 'خالد', LAST_NAME: 'الفهيد' },
    ]);
    const [kinshipTypes] = useState<KinshipType[]>([
        { CODE: 1, DESCR: 'أب' },
        { CODE: 2, DESCR: 'أم' },
        { CODE: 3, DESCR: 'ابن' },
    ]);
    const [services] = useState<Service[]>([
        { code: 1, descr: 'خدمة الأحوال المدنية' },
        { code: 2, descr: 'خدمة السجل العدلي' },
        { code: 3, descr: 'خدمة السجل العقاري' },
        { code: 4, descr: 'خدمة السجل الصناعي' },
    ]);
    const [selectedServices, setSelectedServices] = useState<OperationDetail[]>([]);
    const [operationsList, setOperationsList] = useState<Operation[]>([
        {
            CODE: 1,
            CODE_CLIENT: 1,
            CODE_CLIENT2: 2,
            CODE_KINSHIP: 1,
            SUM_AMOUNT: 500,
            DATE_ACTION: '2024-01-15',
            NOTES: 'عملية تجريبية',
            user_no: 1,
            Client_No: 1,
            First_Name: 'أحمد',
            FATHER_NAME: 'محمد',
            LAST_NAME: 'علي',
            MOTHER_NAME: 'فاطمة'
        }
    ]);

    const [isEditMode, setIsEditMode] = useState(false);
    const [currentRecordIndex, setCurrentRecordIndex] = useQueryState('id', parseAsInteger.withDefault(0));
    const [totalRecords, setTotalRecords] = useState(1);
    const [searchDate, setSearchDate] = useQueryState('date', parseAsString.withDefault(''));
    const [username] = useState('المستخدم الحالي');

    const clientInputRef = useRef<HTMLInputElement>(null);
    const client2InputRef = useRef<HTMLInputElement>(null);


    // Handlers
    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: name === 'SUM_AMOUNT' ? parseInt(value) || 0 : Number(value)
        }));
    };

    const handleSelectChange = (name: string, value: string) => {
        setFormData(prev => ({
            ...prev,
            [name]: parseInt(value) || 0
        }));
    };

    const handleAddClient = () => {
        alert('فتح نموذج إضافة مواطن جديد');
    };

    const handleSearchClient = (type: 'client1' | 'client2') => {
        const searchTerm = type === 'client1'
            ? clientInputRef.current?.value
            : client2InputRef.current?.value;

        if (searchTerm) {
            alert(`البحث عن مواطن: ${searchTerm}`);
        }
    };

    const handleSave = () => {
        if (!formData.CODE_CLIENT || !formData.DATE_ACTION) {
            alert('يرجى ملء الحقول المطلوبة');
            return;
        }

        // eslint-disable-next-line react-hooks/purity
        const timestamp = Math.floor(Date.now() / 1000);
        const newOperation: Operation = {
            CODE: formData.CODE || timestamp,
            CODE_CLIENT: formData.CODE_CLIENT,
            CODE_CLIENT2: formData.CODE_CLIENT2,
            CODE_KINSHIP: formData.CODE_KINSHIP,
            SUM_AMOUNT: formData.SUM_AMOUNT,
            DATE_ACTION: formData.DATE_ACTION,
            NOTES: formData.NOTES,
            user_no: formData.user_no,
            Client_No: formData.CODE_CLIENT,
            First_Name: 'محمود',
            FATHER_NAME: 'سعيد',
            LAST_NAME: 'الخالدي',
            MOTHER_NAME: 'ريم'
        };

        if (isEditMode) {
            const updatedOperations = [...operationsList];
            updatedOperations[currentRecordIndex] = newOperation;
            setOperationsList(updatedOperations);
        } else {
            setOperationsList(prev => [...prev, newOperation]);
            setTotalRecords(prev => prev + 1);
        }

        setIsEditMode(false);
        resetForm();
        alert('تم حفظ العملية بنجاح');
    };

    const handleEdit = () => {
        if (operationsList.length > 0 && currentRecordIndex < operationsList.length) {
            const currentOp = operationsList[currentRecordIndex];
            setFormData({
                CODE: currentOp.CODE,
                CODE_CLIENT: currentOp.CODE_CLIENT,
                CODE_CLIENT2: currentOp.CODE_CLIENT2,
                CODE_KINSHIP: currentOp.CODE_KINSHIP,
                SUM_AMOUNT: currentOp.SUM_AMOUNT,
                DATE_ACTION: currentOp.DATE_ACTION,
                NOTES: currentOp.NOTES,
                user_no: currentOp.user_no,
                Date_Of_work: new Date().toISOString().split('T')[0]
            });
            setIsEditMode(true);
        }
    };

    const handleDelete = () => {
        if (operationsList.length > 0) {
            if (window.confirm('هل أنت متأكد من حذف هذا السجل؟')) {
                const updatedOperations = operationsList.filter((_, index) => index !== currentRecordIndex);
                setOperationsList(updatedOperations);
                setTotalRecords(updatedOperations.length);

                if (currentRecordIndex >= updatedOperations.length) {
                    setCurrentRecordIndex(Math.max(0, updatedOperations.length - 1));
                }

                resetForm();
            }
        }
    };

    const handleCancel = () => {
        setIsEditMode(false);
        resetForm();
    };

    const handleNew = () => {
        setIsEditMode(false);
        resetForm();
    };

    const handleNavigate = (direction: 'first' | 'prev' | 'next' | 'last') => {
        let newIndex = currentRecordIndex;

        switch (direction) {
            case 'first': newIndex = 0; break;
            case 'prev': newIndex = Math.max(0, currentRecordIndex - 1); break;
            case 'next': newIndex = Math.min(operationsList.length - 1, currentRecordIndex + 1); break;
            case 'last': newIndex = operationsList.length - 1; break;
        }

        if (newIndex !== currentRecordIndex && operationsList[newIndex]) {
            setCurrentRecordIndex(newIndex);
            const op = operationsList[newIndex];
            setFormData({
                CODE: op.CODE,
                CODE_CLIENT: op.CODE_CLIENT,
                CODE_CLIENT2: op.CODE_CLIENT2,
                CODE_KINSHIP: op.CODE_KINSHIP,
                SUM_AMOUNT: op.SUM_AMOUNT,
                DATE_ACTION: op.DATE_ACTION,
                NOTES: op.NOTES,
                user_no: op.user_no,
                Date_Of_work: new Date().toISOString().split('T')[0]
            });
        }
    };

    const handleRemoveSelectedService = (code: number) => {
        setSelectedServices(prev => prev.filter(s => s.code !== code));
    };

    const resetForm = () => {
        setFormData({
            CODE: 0,
            CODE_CLIENT: 0,
            CODE_CLIENT2: 0,
            CODE_KINSHIP: 0,
            SUM_AMOUNT: 0,
            DATE_ACTION: new Date().toISOString().split('T')[0],
            NOTES: '',
            user_no: 1,
            Date_Of_work: new Date().toISOString().split('T')[0]
        });
        setSelectedServices([]);
    };

    const getClientFullName = (clientCode: number) => {
        const client = clients.find(c => c.Client_No === clientCode);
        return client ? `${client.FIRST_NAME} ${client.FATHER_NAME} ${client.LAST_NAME}` : 'غير معروف';
    };

    const addService = (service: Service) => {
        const newDetail: OperationDetail = {
            code: Date.now(),
            id_services: service.code,
            num_copy: 1,
            sum_copy: 100,
            sum_amount: 100
        };
        setSelectedServices(prev => [...prev, newDetail]);
    };

    return {
        formData,
        setFormData,
        clients,
        kinshipTypes,
        services,
        selectedServices,
        operationsList,
        isEditMode,
        setIsEditMode,
        currentRecordIndex,
        totalRecords,
        searchDate,
        setSearchDate,
        username,
        clientInputRef,
        client2InputRef,
        
        handleInputChange,
        handleSelectChange,
        handleAddClient,
        handleSearchClient,
        handleSave,
        handleEdit,
        handleDelete,
        handleCancel,
        handleNew,
        handleNavigate,
        handleRemoveSelectedService,
        resetForm,
        getClientFullName,
        addService
    };
};