// DbViewer JavaScript 功能

window.copyToClipboard = async (text) => {
    try {
        // 嘗試使用現代的 Clipboard API
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(text);
            return true;
        } else {
            // 回退到傳統的方法
            return fallbackCopyTextToClipboard(text);
        }
    } catch (err) {
        console.error('複製到剪貼簿失敗:', err);
        return fallbackCopyTextToClipboard(text);
    }
};

function fallbackCopyTextToClipboard(text) {
    try {
        const textArea = document.createElement("textarea");
        textArea.value = text;
        
        // 避免滾動到底部
        textArea.style.top = "0";
        textArea.style.left = "0";
        textArea.style.position = "fixed";
        textArea.style.opacity = "0";
        
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();
        
        const successful = document.execCommand('copy');
        document.body.removeChild(textArea);
        
        return successful;
    } catch (err) {
        console.error('Fallback 複製失敗:', err);
        return false;
    }
}

// 模態框拖拽功能
window.enableModalDrag = (modalSelector) => {
    const modal = document.querySelector(modalSelector);
    const header = modal?.querySelector('.modal-header');
    
    if (!modal || !header) return;
    
    let isDragging = false;
    let startX, startY, startLeft, startTop;
    
    header.addEventListener('mousedown', (e) => {
        if (e.target.closest('.close-btn')) return; // 不要在關閉按鈕上啟動拖拽
        
        isDragging = true;
        startX = e.clientX;
        startY = e.clientY;
        
        const rect = modal.getBoundingClientRect();
        startLeft = rect.left;
        startTop = rect.top;
        
        modal.style.position = 'fixed';
        modal.style.left = startLeft + 'px';
        modal.style.top = startTop + 'px';
        modal.style.margin = '0';
        
        document.addEventListener('mousemove', onMouseMove);
        document.addEventListener('mouseup', onMouseUp);
        
        e.preventDefault();
    });
    
    function onMouseMove(e) {
        if (!isDragging) return;
        
        const deltaX = e.clientX - startX;
        const deltaY = e.clientY - startY;
        
        const newLeft = Math.max(0, Math.min(window.innerWidth - modal.offsetWidth, startLeft + deltaX));
        const newTop = Math.max(0, Math.min(window.innerHeight - modal.offsetHeight, startTop + deltaY));
        
        modal.style.left = newLeft + 'px';
        modal.style.top = newTop + 'px';
    }
    
    function onMouseUp() {
        isDragging = false;
        document.removeEventListener('mousemove', onMouseMove);
        document.removeEventListener('mouseup', onMouseUp);
    }
    
    // 鍵盤快捷鍵支援
    document.addEventListener('keydown', (e) => {
        if (!modal.offsetParent) return; // 模態框不可見時不處理
        
        // Escape 關閉模態框
        if (e.key === 'Escape') {
            const closeBtn = modal.querySelector('.close-btn');
            if (closeBtn) {
                closeBtn.click();
            }
        }
    });
};

// 初始化函數
window.initializeDbViewer = () => {
    console.log('DbViewer JavaScript 已初始化');
    
    // 監聽模態框的出現，自動啟用拖拽功能
    const observer = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
            mutation.addedNodes.forEach((node) => {
                if (node.nodeType === 1 && node.querySelector('.modal-dialog')) {
                    setTimeout(() => {
                        enableModalDrag('.modal-dialog');
                    }, 100);
                }
            });
        });
    });
    
    observer.observe(document.body, { childList: true, subtree: true });
};

// 當 DOM 載入完成時初始化
document.addEventListener('DOMContentLoaded', function() {
    window.initializeDbViewer();
}); 