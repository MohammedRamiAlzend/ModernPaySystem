import { ArrowRight, ChevronLeft, ChevronsLeft, ChevronRight, ChevronsRight } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { useProcessManager } from '@/features/processes/model/process-manager';
import { HeaderToolbar } from './header-toolbar';
import { FilterSection } from './filter-section';
import { MainFormSection } from './main-form-section';
import { ServicesGridSection } from './services-grid-section';
import { SidebarSection } from './sidebar-section';
import { BottomOperationsTable } from './bottom-operations-table';
import { Input } from '@/shared/ui/input';

export const ProcessFormWidget = () => {
    const navigate = useNavigate();
    const {
        formData,
        clients,
        kinshipTypes,
        services,
        selectedServices,
        operationsList,
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
        getClientFullName,
        addService
    } = useProcessManager();

    const handleRowClick = () => {
        handleNavigate('first'); // Reset to first record if needed
        // Set the current record index and form data
        // This would need to be handled differently in a real implementation
    };

    return (
        <AnimatedContainer className="space-y-8 pb-12 text-right">
            <div className="bg-card rounded-2xl border shadow-lg p-6 md:p-8">
                {/* Header & Toolbar */}
                <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6 mb-8 pb-6 border-b">
                    <div className="flex items-center gap-4">
                        <div className="p-3 bg-primary/10 rounded-2xl text-primary">
                            <ArrowRight className="w-6 h-6 rotate-180 cursor-pointer" onClick={() => navigate('/')} />
                        </div>
                        <div>
                            <h2 className="text-2xl font-bold tracking-tight">معاملات سريعة</h2>
                            <p className="text-muted-foreground text-sm mt-1">إدارة وتوثيق المعاملات والخدمات المقدمة</p>
                        </div>
                    </div>

                    <HeaderToolbar
                        onNew={handleNew}
                        onSave={handleSave}
                        onEdit={handleEdit}
                        onDelete={handleDelete}
                        onCancel={handleCancel}
                        onPrint={() => window.print()}
                        onExit={() => navigate('/')}
                    />
                </div>

                {/* Filter Section */}
                <FilterSection 
                    searchDate={searchDate}
                    setSearchDate={setSearchDate}
                />

                <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                    {/* Main Form Section */}
                    <div className="lg:col-span-2 space-y-8">
                        <MainFormSection
                            formData={formData}
                            username={username}
                            clients={clients}
                            kinshipTypes={kinshipTypes}
                            clientInputRef={clientInputRef}
                            client2InputRef={client2InputRef}
                            handleInputChange={handleInputChange}
                            handleSelectChange={handleSelectChange}
                            handleAddClient={handleAddClient}
                            handleSearchClient={handleSearchClient}
                        />

                        {/* Services Grid Section */}
                        <ServicesGridSection
                            services={services}
                            selectedServices={selectedServices}
                            onRemoveService={handleRemoveSelectedService}
                        />
                    </div>

                    {/* Sidebar Section (Available Services & Status) */}
                    <SidebarSection
                        services={services}
                        selectedServices={selectedServices}
                        totalRecords={totalRecords}
                        addService={addService}
                    />
                </div>

                {/* Bottom Operations Table */}
                <BottomOperationsTable
                    operationsList={operationsList}
                    currentRecordIndex={currentRecordIndex}
                    onRowClick={handleRowClick}
                    getClientFullName={getClientFullName}
                />

                {/* Pagination Controls */}
                <div className="mt-6 flex items-center justify-center gap-4 border-t pt-6">
                    <div className="flex items-center bg-muted/50 rounded-xl p-1 gap-1">
                        <button 
                            onClick={() => handleNavigate('first')} 
                            className="p-2 rounded-md hover:bg-accent transition-colors"
                            title="الأول"
                        >
                            <ChevronsRight className="w-4 h-4" />
                        </button>
                        <button 
                            onClick={() => handleNavigate('prev')} 
                            className="p-2 rounded-md hover:bg-accent transition-colors"
                            title="السابق"
                        >
                            <ChevronRight className="w-4 h-4" />
                        </button>
                        <Input
                            className="w-12 h-8 text-center"
                            value={currentRecordIndex + 1}
                            onChange={(e) => {
                                const newIndex = parseInt(e.target.value) - 1;
                                if (!isNaN(newIndex) && newIndex >= 0 && newIndex < operationsList.length) {
                                    handleNavigate('first'); // Reset to first record if needed
                                    // Then navigate to the specific index
                                }
                            }}
                        />
                        <span className="text-sm">من {operationsList.length}</span>
                        <button 
                            onClick={() => handleNavigate('next')} 
                            className="p-2 rounded-md hover:bg-accent transition-colors"
                            title="التالي"
                        >
                            <ChevronLeft className="w-4 h-4" />
                        </button>
                        <button 
                            onClick={() => handleNavigate('last')} 
                            className="p-2 rounded-md hover:bg-accent transition-colors"
                            title="الأخير"
                        >
                            <ChevronsLeft className="w-4 h-4" />
                        </button>
                    </div>
                </div>
            </div>
        </AnimatedContainer>
    );
};